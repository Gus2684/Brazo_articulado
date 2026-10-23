using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador principal del Simulador de Grúa Torre con ArticulationBody en Unity 6.
/// Implementa amortiguación cinemática suave, aceleración progresiva y control anti-sacudidas
/// para eliminar movimientos toscos/bruscos tanto en ratón (sliders) como en teclado (WASD, RF, C/X).
/// </summary>
[DisallowMultipleComponent]
public class SimuladorGruaController : MonoBehaviour
{
    [Header("Árbol ArticulationBody (8 Cuerpos)")]
    [Tooltip("Base de la torre (inamovible)")]
    public ArticulationBody baseTorre;

    [Tooltip("Columna giratoria (Revolute Y)")]
    public ArticulationBody columnaGiro;

    [Tooltip("Pluma horizontal con contrapluma y contrapeso (Fixed)")]
    public ArticulationBody plumaBrazo;

    [Tooltip("Carro de desplazamiento horizontal (Prismatic Z)")]
    public ArticulationBody carro;

    [Tooltip("Cable de izaje vertical (Prismatic Y)")]
    public ArticulationBody cableIzaje;

    [Tooltip("Cabezal pendular pasivo libre (Spherical)")]
    public ArticulationBody cabezalPendulo;

    [Tooltip("Actuador de mordaza izquierda (Prismatic X)")]
    public ArticulationBody dedoIzq;

    [Tooltip("Actuador de mordaza derecha (Prismatic X)")]
    public ArticulationBody dedoDer;

    [Header("Controles Deslizantes (UI Sliders)")]
    public Slider sliderGiro;
    public Slider sliderCarro;
    public Slider sliderCable;
    public Slider sliderPinza;

    [Header("Displays de Telemetría (Textos UI)")]
    public Text textGiro;
    public Text textCarro;
    public Text textCable;
    public Text textPinza;
    public Text textPendulo;
    public Text textMision;

    [Header("Botones de Comando")]
    public Button botonCambiarCamara;
    public Button botonCentrar;
    public Button botonModoSuave;
    public Button botonReiniciar;

    [Header("Sistema Multi-Cámara")]
    public Camera camaraGeneral;
    public Camera camaraCabina;
    public Camera camaraGancho;
    public Text textModoCamara;

    [Header("Cargas y Detección de Misión en Mesa")]
    public Rigidbody[] cajasCarga;
    [Tooltip("Mesa de trabajo de destino para depositar las cajas")]
    public Transform plataformaDestino;

    [Header("Cinemática Suave & Anti-Sacudidas (Suavizado de Inercia)")]
    [Tooltip("Activa el filtro cinemático suave con aceleración progresiva y frenado gradual")]
    public bool suavizadoActivo = true;

    [Tooltip("Calibra la fuerza de la garra para sujetar cargas de 2kg sin expulsión violenta")]
    public bool calibrarFuerzaPinza = true;

    [Tooltip("Aumenta la amortiguación del péndulo esférico para evitar oscilaciones descontroladas")]
    public bool estabilizarPendulo = true;

    [Tooltip("Tiempo de suavizado para el giro de la columna (segundos)")]
    [Range(0.02f, 0.5f)]
    public float tiempoSuavizadoGiro = 0.08f;
    public float velocidadMaxGiro = 22f; // grados/seg

    [Tooltip("Tiempo de suavizado para el carro (segundos)")]
    [Range(0.02f, 0.5f)]
    public float tiempoSuavizadoCarro = 0.08f;
    public float velocidadMaxCarro = 0.50f; // m/s

    [Tooltip("Tiempo de suavizado para el cable (segundos)")]
    [Range(0.02f, 0.5f)]
    public float tiempoSuavizadoCable = 0.08f;
    public float velocidadMaxCable = 0.45f; // m/s

    [Tooltip("Tiempo de suavizado para la pinza (segundos)")]
    [Range(0.02f, 0.5f)]
    public float tiempoSuavizadoPinza = 0.05f;
    public float velocidadMaxPinza = 0.14f; // m/s

    [Header("Velocidades y Respuesta Inmediata (Teclado)")]
    [Tooltip("Velocidad de giro por teclado (grados/seg) - Moderada y controlada")]
    public float velocidadGiroTeclado = 18f;
    [Tooltip("Velocidad del carro por teclado (m/s) - Desplazamiento sereno")]
    public float velocidadCarroTeclado = 0.40f;
    [Tooltip("Velocidad del cable por teclado (m/s) - Izaje preciso")]
    public float velocidadCableTeclado = 0.35f;
    [Tooltip("Velocidad de apertura/cierre de la pinza por teclado (m/s)")]
    public float velocidadPinzaTeclado = 0.10f;
    [Tooltip("Respuesta de arranque y frenado del teclado (alto = respuesta instantánea sin retrasos)")]
    public float aceleracionTeclado = 18f;

    // Estados internos del suavizado cinemático
    private float consignaGiroActual = 0f;
    private float consignaCarroActual = 0f;
    private float consignaCableActual = 0f;
    private float consignaPinzaActual = 0f;

    private float velDerivGiro = 0f;
    private float velDerivCarro = 0f;
    private float velDerivCable = 0f;
    private float velDerivPinza = 0f;

    // Rampas de entrada de teclado (0 a 1 progresivo)
    private float rampaInputGiro = 0f;
    private float rampaInputCarro = 0f;
    private float rampaInputCable = 0f;
    private float rampaInputPinza = 0f;

    private int indiceCamaraActual = 0;
    private int cajasEntregadas = 0;
    private Slider sliderEnArrastreMouse = null;

    private void Awake()
    {
        // Mayor precisión del solver de PhysX para estabilidad de contacto sin rebotes
        Physics.defaultSolverIterations = 20;
        Physics.defaultSolverVelocityIterations = 10;
    }

    private void Start()
    {
        CalibrarActuadoresFisicos();
        ConfigurarBotones();
        ActualizarVistaCamaras();
        ActualizarBotonModoSuave();
        SincronizarTargetsIniciales();
    }

    /// <summary>
    /// Ajusta las fuerzas físicas para que la máquina opere con elegancia e inercia realista:
    /// evita que 800N proyecten una caja de 2kg y estabiliza el gancho esférico pasivo.
    /// </summary>
    private void CalibrarActuadoresFisicos()
    {
        if (calibrarFuerzaPinza)
        {
            if (dedoIzq != null)
            {
                ArticulationDrive dIzq = dedoIzq.xDrive;
                dIzq.stiffness = 5000f;
                dIzq.damping = 600f;
                dIzq.forceLimit = 140f; // ~14 kg de agarre: suficiente para 2kg sin expulsión elástica
                dedoIzq.xDrive = dIzq;
            }

            if (dedoDer != null)
            {
                ArticulationDrive dDer = dedoDer.xDrive;
                dDer.stiffness = 5000f;
                dDer.damping = 600f;
                dDer.forceLimit = 140f;
                dedoDer.xDrive = dDer;
            }
        }

        if (estabilizarPendulo && cabezalPendulo != null)
        {
            // Amortiguación armónica para que el cabezal oscile con gravedad pero no tiemble descontrolado
            ArticulationDrive dy = cabezalPendulo.yDrive;
            dy.damping = 28f;
            cabezalPendulo.yDrive = dy;

            ArticulationDrive dz = cabezalPendulo.zDrive;
            dz.damping = 28f;
            cabezalPendulo.zDrive = dz;
        }
    }

    private void ConfigurarBotones()
    {
        if (botonCambiarCamara != null)
        {
            botonCambiarCamara.onClick.RemoveListener(CiclarCamara);
            botonCambiarCamara.onClick.AddListener(CiclarCamara);
        }

        if (botonCentrar != null)
        {
            botonCentrar.onClick.RemoveListener(CentrarGrua);
            botonCentrar.onClick.AddListener(CentrarGrua);
        }

        if (botonModoSuave != null)
        {
            botonModoSuave.onClick.RemoveListener(AlternarModoSuave);
            botonModoSuave.onClick.AddListener(AlternarModoSuave);
        }

        if (botonReiniciar != null)
        {
            botonReiniciar.onClick.RemoveListener(ReiniciarEscena);
            botonReiniciar.onClick.AddListener(ReiniciarEscena);
        }
    }

    public void AlternarModoSuave()
    {
        suavizadoActivo = !suavizadoActivo;
        ActualizarBotonModoSuave();
    }

    private void ActualizarBotonModoSuave()
    {
        if (botonModoSuave != null)
        {
            Text txt = botonModoSuave.GetComponentInChildren<Text>();
            Image img = botonModoSuave.GetComponent<Image>();
            if (suavizadoActivo)
            {
                if (txt != null) txt.text = "🌿 Modo: Suave [T]";
                if (img != null) img.color = new Color(0.15f, 0.55f, 0.45f);
            }
            else
            {
                if (txt != null) txt.text = "⚡ Modo: Directo [T]";
                if (img != null) img.color = new Color(0.75f, 0.45f, 0.15f);
            }
        }
    }

    private void SincronizarTargetsIniciales()
    {
        if (sliderGiro != null) consignaGiroActual = sliderGiro.value;
        if (sliderCarro != null) consignaCarroActual = sliderCarro.value;
        if (sliderCable != null) consignaCableActual = sliderCable.value;
        if (sliderPinza != null) consignaPinzaActual = sliderPinza.value;

        if (columnaGiro != null) SetDriveTarget(columnaGiro, consignaGiroActual);
        if (carro != null) SetDriveTarget(carro, consignaCarroActual);
        if (cableIzaje != null) SetDriveTarget(cableIzaje, consignaCableActual);
        if (sliderPinza != null)
        {
            if (dedoIzq != null) SetDriveTarget(dedoIzq, consignaPinzaActual);
            if (dedoDer != null) SetDriveTarget(dedoDer, -consignaPinzaActual);
        }
    }

    private void Update()
    {
        // 1. Entrada de ratón en Sliders (clic, arrastre y rueda de desplazamiento)
        ProcesarArrastreMouseSliders();

        // 2. Entrada progresiva por teclado con rampa suave de aceleración y frenado
        ProcesarEntradaTeclado();

        // 3. Displays de telemetría en tiempo real
        ActualizarTelemetriaUI();

        // 4. Comprobar cajas en la mesa de trabajo
        VerificarObjetivoMision();
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (suavizadoActivo)
        {
            // Filtro cinemático de orden superior (SmoothDamp):
            // Suaviza cualquier cambio abrupto o tirón del ratón/teclado para mover la masa con inercia
            if (columnaGiro != null && sliderGiro != null)
            {
                consignaGiroActual = Mathf.SmoothDamp(consignaGiroActual, sliderGiro.value, ref velDerivGiro, tiempoSuavizadoGiro, velocidadMaxGiro, dt);
                SetDriveTarget(columnaGiro, consignaGiroActual);
            }

            if (carro != null && sliderCarro != null)
            {
                consignaCarroActual = Mathf.SmoothDamp(consignaCarroActual, sliderCarro.value, ref velDerivCarro, tiempoSuavizadoCarro, velocidadMaxCarro, dt);
                SetDriveTarget(carro, consignaCarroActual);
            }

            if (cableIzaje != null && sliderCable != null)
            {
                consignaCableActual = Mathf.SmoothDamp(consignaCableActual, sliderCable.value, ref velDerivCable, tiempoSuavizadoCable, velocidadMaxCable, dt);
                SetDriveTarget(cableIzaje, consignaCableActual);
            }

            if (sliderPinza != null)
            {
                consignaPinzaActual = Mathf.SmoothDamp(consignaPinzaActual, sliderPinza.value, ref velDerivPinza, tiempoSuavizadoPinza, velocidadMaxPinza, dt);
                if (dedoIzq != null) SetDriveTarget(dedoIzq, consignaPinzaActual);
                if (dedoDer != null) SetDriveTarget(dedoDer, -consignaPinzaActual);
            }
        }
        else
        {
            // Modo Directo sin suavizado (respuesta rígida instantánea)
            if (columnaGiro != null && sliderGiro != null) SetDriveTarget(columnaGiro, sliderGiro.value);
            if (carro != null && sliderCarro != null) SetDriveTarget(carro, sliderCarro.value);
            if (cableIzaje != null && sliderCable != null) SetDriveTarget(cableIzaje, sliderCable.value);
            if (sliderPinza != null)
            {
                float val = sliderPinza.value;
                if (dedoIzq != null) SetDriveTarget(dedoIzq, val);
                if (dedoDer != null) SetDriveTarget(dedoDer, -val);
            }
        }
    }

    /// <summary>
    /// Permite hacer clic o arrastrar cualquier slider directamente con el mouse.
    /// Funciona de forma totalmente independiente del estado del EventSystem.
    /// </summary>
    private void ProcesarArrastreMouseSliders()
    {
        Vector2 mousePos = ObtenerPosicionMouse();
        bool clickIniciado = EsClickIzquierdoIniciado();
        bool clickSostenido = EsClickIzquierdoPresionado();
        float scrollY = ObtenerScrollMouse();

        Slider[] listaSliders = new Slider[] { sliderGiro, sliderCarro, sliderCable, sliderPinza };

        // 1. Si el usuario hace clic primario, identificar qué slider se seleccionó
        if (clickIniciado)
        {
            sliderEnArrastreMouse = null;
            for (int i = 0; i < listaSliders.Length; i++)
            {
                Slider s = listaSliders[i];
                if (s != null)
                {
                    RectTransform rt = s.GetComponent<RectTransform>();
                    if (rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos))
                    {
                        sliderEnArrastreMouse = s;
                        ActualizarValorSliderPorPosicionMouse(s, rt, mousePos);
                        break;
                    }
                }
            }
        }
        // 2. Si se mantiene presionado el botón izquierdo sobre el slider activo, arrastrar el valor
        else if (clickSostenido && sliderEnArrastreMouse != null)
        {
            RectTransform rt = sliderEnArrastreMouse.GetComponent<RectTransform>();
            if (rt != null)
            {
                ActualizarValorSliderPorPosicionMouse(sliderEnArrastreMouse, rt, mousePos);
            }
        }
        // 3. Al soltar el clic, finalizar el arrastre
        else if (!clickSostenido)
        {
            sliderEnArrastreMouse = null;
        }

        // 4. Soporte para rueda de desplazamiento (scroll) cuando el cursor sobrevuela un slider
        if (Mathf.Abs(scrollY) > 0.01f)
        {
            for (int i = 0; i < listaSliders.Length; i++)
            {
                Slider s = listaSliders[i];
                if (s != null)
                {
                    RectTransform rt = s.GetComponent<RectTransform>();
                    if (rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos))
                    {
                        float rango = s.maxValue - s.minValue;
                        float delta = rango * 0.025f * Mathf.Sign(scrollY);
                        s.value = Mathf.Clamp(s.value + delta, s.minValue, s.maxValue);
                        break;
                    }
                }
            }
        }
    }

    private static void ActualizarValorSliderPorPosicionMouse(Slider s, RectTransform rt, Vector2 screenPos)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, null, out Vector2 localPt))
        {
            float width = rt.rect.width;
            if (width > 0f)
            {
                float t = Mathf.Clamp01((localPt.x - rt.rect.xMin) / width);
                s.value = Mathf.Lerp(s.minValue, s.maxValue, t);
            }
        }
    }

    private Vector2 ObtenerPosicionMouse()
    {
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            return UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        }
#endif
        return Vector2.zero;
    }

    private bool EsClickIzquierdoPresionado()
    {
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            return UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
        }
#endif
        return false;
    }

    private bool EsClickIzquierdoIniciado()
    {
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            return UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;
        }
#endif
        return false;
    }

    private float ObtenerScrollMouse()
    {
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            return UnityEngine.InputSystem.Mouse.current.scroll.ReadValue().y;
        }
#endif
        return 0f;
    }

    /// <summary>
    /// Control por teclado continuo con aceleración y desaceleración progresiva (soft start/stop).
    /// </summary>
    private void ProcesarEntradaTeclado()
    {
        float dt = Time.deltaTime;

#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;

        // Alternar vista de cámara con tecla V
        if (kb.vKey.wasPressedThisFrame)
        {
            CiclarCamara();
        }

        // Alternar Modo Suave / Directo con tecla T
        if (kb.tKey.wasPressedThisFrame)
        {
            AlternarModoSuave();
        }

        // 1. Giro de Torre: [A] Izquierda / [D] Derecha con rampa progresiva
        float targetDirGiro = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) targetDirGiro -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) targetDirGiro += 1f;

        rampaInputGiro = Mathf.MoveTowards(rampaInputGiro, targetDirGiro, dt * aceleracionTeclado);
        if (Mathf.Abs(rampaInputGiro) > 0.001f && sliderGiro != null)
        {
            sliderGiro.value = Mathf.Clamp(sliderGiro.value + rampaInputGiro * velocidadGiroTeclado * dt, sliderGiro.minValue, sliderGiro.maxValue);
        }

        // 2. Carro en Pluma: [W] Adelante / [S] Atrás con rampa progresiva
        float targetDirCarro = 0f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) targetDirCarro += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) targetDirCarro -= 1f;

        rampaInputCarro = Mathf.MoveTowards(rampaInputCarro, targetDirCarro, dt * aceleracionTeclado);
        if (Mathf.Abs(rampaInputCarro) > 0.001f && sliderCarro != null)
        {
            sliderCarro.value = Mathf.Clamp(sliderCarro.value + rampaInputCarro * velocidadCarroTeclado * dt, sliderCarro.minValue, sliderCarro.maxValue);
        }

        // 3. Cable de Izaje: [R] Subir / [F] Bajar con rampa progresiva
        float targetDirCable = 0f;
        if (kb.rKey.isPressed) targetDirCable += 1f;
        if (kb.fKey.isPressed) targetDirCable -= 1f;

        rampaInputCable = Mathf.MoveTowards(rampaInputCable, targetDirCable, dt * aceleracionTeclado);
        if (Mathf.Abs(rampaInputCable) > 0.001f && sliderCable != null)
        {
            sliderCable.value = Mathf.Clamp(sliderCable.value + rampaInputCable * velocidadCableTeclado * dt, sliderCable.minValue, sliderCable.maxValue);
        }

        // 4. Pinza / Garra: [C] o [Espacio] Cerrar / [X] Abrir con rampa progresiva
        float targetDirPinza = 0f;
        if (kb.cKey.isPressed || kb.spaceKey.isPressed) targetDirPinza += 1f;
        if (kb.xKey.isPressed) targetDirPinza -= 1f;

        rampaInputPinza = Mathf.MoveTowards(rampaInputPinza, targetDirPinza, dt * aceleracionTeclado);
        if (Mathf.Abs(rampaInputPinza) > 0.001f && sliderPinza != null)
        {
            sliderPinza.value = Mathf.Clamp(sliderPinza.value + rampaInputPinza * velocidadPinzaTeclado * dt, sliderPinza.minValue, sliderPinza.maxValue);
        }
#endif
    }

    /// <summary>
    /// Cambia cíclicamente entre las 3 perspectivas de cámara disponibles.
    /// </summary>
    public void CiclarCamara()
    {
        indiceCamaraActual = (indiceCamaraActual + 1) % 3;
        ActualizarVistaCamaras();
    }

    private void ActualizarVistaCamaras()
    {
        if (camaraGeneral != null) camaraGeneral.enabled = (indiceCamaraActual == 0);
        if (camaraCabina != null) camaraCabina.enabled = (indiceCamaraActual == 1);
        if (camaraGancho != null) camaraGancho.enabled = (indiceCamaraActual == 2);

        if (textModoCamara != null)
        {
            switch (indiceCamaraActual)
            {
                case 0:
                    textModoCamara.text = "🎥 Cámara: <b>[1/3] Vista Isométrica</b> <color=#90A4BE>(V)</color>";
                    break;
                case 1:
                    textModoCamara.text = "🎥 Cámara: <b>[2/3] Cabina Operador</b> <color=#90A4BE>(V)</color>";
                    break;
                case 2:
                    textModoCamara.text = "🎥 Cámara: <b>[3/3] Cenital Gancho</b> <color=#90A4BE>(V)</color>";
                    break;
            }
        }
    }

    /// <summary>
    /// Restablece suavemente todas las consignas de los actuadores a su posición neutra de reposo.
    /// </summary>
    public void CentrarGrua()
    {
        if (sliderGiro != null) sliderGiro.value = 0f;
        if (sliderCarro != null) sliderCarro.value = 0f;
        if (sliderCable != null) sliderCable.value = 0f;
        if (sliderPinza != null) sliderPinza.value = 0f;
    }

    private static void SetDriveTarget(ArticulationBody body, float target)
    {
        if (body == null) return;
        ArticulationDrive drive = body.xDrive;
        drive.target = target;
        body.xDrive = drive;
    }

    private static float ReadJointPosition(ArticulationBody body)
    {
        if (body != null && body.jointPosition.dofCount > 0)
        {
            return body.jointPosition[0];
        }
        return 0f;
    }

    private void ActualizarTelemetriaUI()
    {
        string modoTag = suavizadoActivo ? "<color=#4EFEB3>🌿 Suave</color>" : "<color=#FFA500>⚡ Directo</color>";

        // 1. Columna Giro (Revolute)
        if (textGiro != null)
        {
            float target = sliderGiro != null ? sliderGiro.value : 0f;
            float realDeg = ReadJointPosition(columnaGiro) * Mathf.Rad2Deg;
            textGiro.text = $"<b>Columna Giro</b> (Revolute Y) [{modoTag}]\nTarget: <color=#F8C826>{target:F1}°</color>   |   Real: <color=#66FF88>{realDeg:F1}°</color>";
        }

        // 2. Carro Pluma (Prismatic)
        if (textCarro != null)
        {
            float target = sliderCarro != null ? sliderCarro.value : 0f;
            float realM = ReadJointPosition(carro);
            textCarro.text = $"<b>Carro Guía</b> (Prismatic Z) [{modoTag}]\nTarget: <color=#F8C826>{target:F2}m</color>   |   Real: <color=#66FF88>{realM:F2}m</color>";
        }

        // 3. Cable Izaje (Prismatic)
        if (textCable != null)
        {
            float target = sliderCable != null ? sliderCable.value : 0f;
            float realM = ReadJointPosition(cableIzaje);
            textCable.text = $"<b>Cable Izaje</b> (Prismatic Y) [{modoTag}]\nTarget: <color=#F8C826>{target:F2}m</color>   |   Real: <color=#66FF88>{realM:F2}m</color>";
        }

        // 4. Pinza Gripper (Prismatic simétrico)
        if (textPinza != null)
        {
            float target = sliderPinza != null ? sliderPinza.value : 0f;
            float realIzq = ReadJointPosition(dedoIzq);
            float realDer = ReadJointPosition(dedoDer);
            textPinza.text = $"<b>Garra Robótica</b> (Prismatic X) [{modoTag}]\nTarget: <color=#F8C826>{target:F2}m</color>   |   Real: <color=#66FF88>L:{realIzq:F2}m / R:{realDer:F2}m</color>";
        }

        // 5. Cabezal Péndulo (Spherical Pasivo)
        if (textPendulo != null)
        {
            if (cabezalPendulo != null)
            {
                float swingY = 0f;
                float swingZ = 0f;

                if (cabezalPendulo.jointPosition.dofCount >= 2)
                {
                    swingY = cabezalPendulo.jointPosition[0] * Mathf.Rad2Deg;
                    swingZ = cabezalPendulo.jointPosition[1] * Mathf.Rad2Deg;
                }
                else if (cabezalPendulo.jointPosition.dofCount == 1)
                {
                    swingY = cabezalPendulo.jointPosition[0] * Mathf.Rad2Deg;
                }

                float inclinacionTotal = Mathf.Sqrt(swingY * swingY + swingZ * swingZ);
                textPendulo.text = $"⏱️ <b>Péndulo Inercial</b> (Spherical Pasivo)\nInclinación: <color=#50D0FF>{inclinacionTotal:F1}°</color> (SwingY: {swingY:F1}°, SwingZ: {swingZ:F1}°)";
            }
        }
    }

    private void VerificarObjetivoMision()
    {
        if (cajasCarga == null || cajasCarga.Length == 0 || plataformaDestino == null) return;

        Vector3 mesaPos = plataformaDestino.position;
        int entregadas = 0;

        for (int i = 0; i < cajasCarga.Length; i++)
        {
            if (cajasCarga[i] == null) continue;
            Vector3 cPos = cajasCarga[i].position;

            bool enX = Mathf.Abs(cPos.x - mesaPos.x) <= 1.15f;
            bool enZ = Mathf.Abs(cPos.z - mesaPos.z) <= 1.05f;
            bool enY = cPos.y >= 0.55f && cPos.y <= 2.2f;

            if (enX && enZ && enY)
            {
                entregadas++;
            }
        }

        cajasEntregadas = entregadas;

        if (textMision != null)
        {
            if (cajasEntregadas >= cajasCarga.Length)
            {
                textMision.text = $"🛠️ <b>Misión:</b> <color=#55FF88>¡{cajasEntregadas}/{cajasCarga.Length} en Mesa! MISIÓN CUMPLIDA ✓</color>";
            }
            else
            {
                textMision.text = $"🛠️ <b>Misión:</b> Cajas en Mesa de Trabajo: <color=#F8C826>{cajasEntregadas} / {cajasCarga.Length}</color>";
            }
        }
    }

    public void ReiniciarEscena()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }
}