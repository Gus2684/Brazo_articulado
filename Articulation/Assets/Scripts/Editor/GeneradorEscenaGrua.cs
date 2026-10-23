using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Generador de Escena Avanzado para el Trabajo Práctico de ArticulationBody en Unity 6.
/// Incluye Grúa Torre industrial, Garra Robótica estilizada (mandíbulas con uñas de apoyo,
/// almohadillas de goma vulcanizada y cilindro hidráulico central), Mesa de Trabajo Industrial de carga,
/// sistema multi-cámara y telemetría de misión.
/// </summary>
public static class GeneradorEscenaGrua
{
    private const string MenuPath = "TP Articulation/Generar Escena Completa";

    [MenuItem(MenuPath, false, 1)]
    public static void GenerarEscenaCompleta()
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Generar Escena Grúa Torre con Garra y Mesa");
        int undoGroup = Undo.GetCurrentGroup();

        Debug.Log("<color=cyan>[TP Articulation]</color> Iniciando generación de la escena con Garra Robótica y Mesa de Trabajo...");

        // 1. Limpieza de elementos previos generados
        LimpiarElementosPrevios();

        // 2. Configuración del Solver de Física
        Physics.defaultSolverIterations = 16;
        Physics.defaultSolverVelocityIterations = 8;

        // 3. Crear PhysicsMaterial de alta fricción (1.0 estático/dinámico, combine Maximum)
        PhysicsMaterial physMatFriccion = ObtenerOCrearPhysicsMaterial();

        // 4. Paleta de Materiales Industriales
        Material matGruaAmarillo = ObtenerOCrearMaterial("Mat_Grua_AmarilloIndustrial", new Color(0.96f, 0.68f, 0.08f), 0.35f, 0.15f);
        Material matGruaChasis = ObtenerOCrearMaterial("Mat_Grua_ChasisOscuro", new Color(0.18f, 0.20f, 0.24f), 0.45f, 0.3f);
        Material matGruaContrapeso = ObtenerOCrearMaterial("Mat_Grua_ContrapesoHormigon", new Color(0.46f, 0.47f, 0.50f), 0.1f, 0.0f);
        Material matGruaCabina = ObtenerOCrearMaterial("Mat_Grua_CabinaVidrio", new Color(0.25f, 0.60f, 0.85f), 0.9f, 0.2f);
        Material matGruaCable = ObtenerOCrearMaterial("Mat_Grua_CableAcero", new Color(0.42f, 0.44f, 0.48f), 0.3f, 0.6f);

        // Nuevos Materiales de la Garra Robótica
        Material matGarraCuerpo = ObtenerOCrearMaterial("Mat_Garra_AzulCobalto", new Color(0.08f, 0.52f, 0.85f), 0.4f, 0.25f);
        Material matGarraDetalle = ObtenerOCrearMaterial("Mat_Garra_DetalleAmarillo", new Color(0.98f, 0.75f, 0.08f), 0.35f, 0.1f);
        Material matGarraGoma = ObtenerOCrearMaterial("Mat_Garra_GomaVulcanizada", new Color(0.12f, 0.12f, 0.14f), 0.1f, 0.0f);
        Material matGarraCromo = ObtenerOCrearMaterial("Mat_Garra_CromoPiston", new Color(0.78f, 0.80f, 0.85f), 0.85f, 0.7f);

        // Materiales de la Mesa de Trabajo y Entorno
        Material matPiso = ObtenerOCrearMaterial("Mat_PisoHormigon", new Color(0.24f, 0.25f, 0.28f), 0.15f, 0.0f);
        Material matPeligro = ObtenerOCrearMaterial("Mat_BordeSeguridadAmarillo", new Color(0.96f, 0.78f, 0.10f), 0.2f, 0.0f);
        Material matMesaTablero = ObtenerOCrearMaterial("Mat_Mesa_TableroAcero", new Color(0.32f, 0.36f, 0.42f), 0.35f, 0.2f);
        Material matMesaEstructura = ObtenerOCrearMaterial("Mat_Mesa_EstructuraPatas", new Color(0.14f, 0.17f, 0.22f), 0.4f, 0.3f);
        Material matMesaBarandilla = ObtenerOCrearMaterial("Mat_Mesa_BarandillaAmarilla", new Color(0.95f, 0.72f, 0.10f), 0.3f, 0.1f);

        // Materiales distintivos para cada caja
        Material matCaja1 = ObtenerOCrearMaterial("Mat_Caja_ContenedorRojo", new Color(0.78f, 0.26f, 0.20f), 0.25f, 0.05f);
        Material matCaja2 = ObtenerOCrearMaterial("Mat_Caja_ContenedorAzul", new Color(0.20f, 0.44f, 0.75f), 0.25f, 0.05f);
        Material matCaja3 = ObtenerOCrearMaterial("Mat_Caja_ContenedorMadera", new Color(0.85f, 0.58f, 0.22f), 0.15f, 0.0f);

        // 5. Configurar Iluminación de la escena
        ConfigurarIluminacion();

        // 6. Construir Entorno (Piso, Mesa de Trabajo Industrial y Cajas de Carga)
        GameObject goPiso = ConstruirPiso(matPiso, matPeligro, physMatFriccion);
        GameObject goMesa = ConstruirMesaTrabajo(matMesaTablero, matMesaEstructura, matMesaBarandilla, physMatFriccion);
        Rigidbody[] arrayCajas = ConstruirCajasCarga(matCaja1, matCaja2, matCaja3, physMatFriccion);

        // 7. Construir Árbol de 8 ArticulationBody con la nueva Garra Robótica y cámaras
        ArticulationBody abBase, abColumna, abPluma, abCarro, abCable, abCabezal, abDedoIzq, abDedoDer;
        Camera camaraCabina, camaraGancho;

        GameObject goBase = ConstruirArbolGrua(
            matGruaAmarillo,
            matGruaChasis,
            matGruaContrapeso,
            matGruaCabina,
            matGruaCable,
            matGarraCuerpo,
            matGarraDetalle,
            matGarraGoma,
            matGarraCromo,
            physMatFriccion,
            out abBase,
            out abColumna,
            out abPluma,
            out abCarro,
            out abCable,
            out abCabezal,
            out abDedoIzq,
            out abDedoDer,
            out camaraCabina,
            out camaraGancho
        );

        // Cámara General Principal
        Camera camaraGeneral = ConfigurarCamaraPrincipal();

        // 8. Construir UI Profesional con controles grandes, panel lateral estilizado y telemetría
        Slider sliderGiro, sliderCarro, sliderCable, sliderPinza;
        Text textGiro, textCarro, textCable, textPinza, textPendulo, textMision, textModoCamara;
        Button botonCambiarCamara, botonCentrar, botonModoSuave, botonReiniciar;

        ConstruirUI(
            out sliderGiro,
            out sliderCarro,
            out sliderCable,
            out sliderPinza,
            out textGiro,
            out textCarro,
            out textCable,
            out textPinza,
            out textPendulo,
            out textMision,
            out textModoCamara,
            out botonCambiarCamara,
            out botonCentrar,
            out botonModoSuave,
            out botonReiniciar
        );

        // 9. Configurar y Conectar el Controlador Principal
        SimuladorGruaController controller = goBase.AddComponent<SimuladorGruaController>();
        controller.baseTorre = abBase;
        controller.columnaGiro = abColumna;
        controller.plumaBrazo = abPluma;
        controller.carro = abCarro;
        controller.cableIzaje = abCable;
        controller.cabezalPendulo = abCabezal;
        controller.dedoIzq = abDedoIzq;
        controller.dedoDer = abDedoDer;

        controller.sliderGiro = sliderGiro;
        controller.sliderCarro = sliderCarro;
        controller.sliderCable = sliderCable;
        controller.sliderPinza = sliderPinza;

        controller.textGiro = textGiro;
        controller.textCarro = textCarro;
        controller.textCable = textCable;
        controller.textPinza = textPinza;
        controller.textPendulo = textPendulo;
        controller.textMision = textMision;
        controller.textModoCamara = textModoCamara;

        controller.botonCambiarCamara = botonCambiarCamara;
        controller.botonCentrar = botonCentrar;
        controller.botonModoSuave = botonModoSuave;
        controller.botonReiniciar = botonReiniciar;

        controller.camaraGeneral = camaraGeneral;
        controller.camaraCabina = camaraCabina;
        controller.camaraGancho = camaraGancho;

        controller.cajasCarga = arrayCajas;
        controller.plataformaDestino = goMesa.transform;

        // 10. Finalizar y seleccionar la grúa en el Inspector
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Undo.CollapseUndoOperations(undoGroup);
        Selection.activeGameObject = goBase;
        EditorGUIUtility.PingObject(goBase);

        Debug.Log("<color=green>[TP Articulation] ¡Grúa Torre generada con éxito!</color> Garra Robótica estilizada y Mesa de Trabajo configuradas.");
    }

    private static void LimpiarElementosPrevios()
    {
        string[] nombres = new string[]
        {
            "1_Base_Torre",
            "Entorno_Grua",
            "Canvas_GruaUI",
            "EventSystem"
        };

        foreach (string n in nombres)
        {
            GameObject obj = GameObject.Find(n);
            if (obj != null)
            {
                Undo.DestroyObjectImmediate(obj);
            }
        }

        SimuladorGruaController[] viejos = Object.FindObjectsByType<SimuladorGruaController>(FindObjectsInactive.Include);
        foreach (var c in viejos)
        {
            if (c != null && c.gameObject != null)
            {
                Undo.DestroyObjectImmediate(c.gameObject);
            }
        }
    }

    /// <summary>
    /// Construye el árbol cinemático de 8 ArticulationBody con la Garra Robótica estilizada.
    /// Respeta la regla crítica: localScale = Vector3.one y parentAnchorPosition = localPosition.
    /// </summary>
    private static GameObject ConstruirArbolGrua(
        Material matAmarillo,
        Material matChasis,
        Material matContrapeso,
        Material matCabina,
        Material matCable,
        Material matGarraCuerpo,
        Material matGarraDetalle,
        Material matGarraGoma,
        Material matGarraCromo,
        PhysicsMaterial physMatPinza,
        out ArticulationBody abBase,
        out ArticulationBody abColumna,
        out ArticulationBody abPluma,
        out ArticulationBody abCarro,
        out ArticulationBody abCable,
        out ArticulationBody abCabezal,
        out ArticulationBody abDedoIzq,
        out ArticulationBody abDedoDer,
        out Camera camaraCabina,
        out Camera camaraGancho)
    {
        // -------------------------------------------------------------
        // 1. 1_Base_Torre (Raíz): Inamovible, Mass=500. BoxCollider (1.5, 1, 1.5) en Y=0.5
        // -------------------------------------------------------------
        Vector3 posBase = Vector3.zero;
        GameObject goBase = new GameObject("1_Base_Torre");
        Undo.RegisterCreatedObjectUndo(goBase, "Crear 1_Base_Torre");
        goBase.transform.position = posBase;
        goBase.transform.rotation = Quaternion.identity;
        goBase.transform.localScale = Vector3.one;

        BoxCollider colBase = goBase.AddComponent<BoxCollider>();
        colBase.center = new Vector3(0f, 0.5f, 0f);
        colBase.size = new Vector3(1.5f, 1f, 1.5f);

        abBase = goBase.AddComponent<ArticulationBody>();
        abBase.immovable = true;
        abBase.mass = 500f;
        abBase.solverIterations = 16;
        abBase.solverVelocityIterations = 8;

        GameObject visualBase = new GameObject("Visual");
        visualBase.transform.SetParent(goBase.transform, false);
        CrearVisualCubo(visualBase.transform, "BloqueCentral", new Vector3(0f, 0.5f, 0f), new Vector3(1.5f, 1f, 1.5f), matAmarillo);
        CrearVisualCubo(visualBase.transform, "Pata_1", new Vector3(-0.95f, 0.15f, -0.95f), new Vector3(0.5f, 0.3f, 0.5f), matChasis);
        CrearVisualCubo(visualBase.transform, "Pata_2", new Vector3(0.95f, 0.15f, -0.95f), new Vector3(0.5f, 0.3f, 0.5f), matChasis);
        CrearVisualCubo(visualBase.transform, "Pata_3", new Vector3(-0.95f, 0.15f, 0.95f), new Vector3(0.5f, 0.3f, 0.5f), matChasis);
        CrearVisualCubo(visualBase.transform, "Pata_4", new Vector3(0.95f, 0.15f, 0.95f), new Vector3(0.5f, 0.3f, 0.5f), matChasis);

        // -------------------------------------------------------------
        // 2. 2_Columna_Giro (Hijo de 1): Pos local (0, 1, 0), Joint Revolute en Y.
        //    Mass=150. BoxCollider (0.6, 4, 0.6) en Y=2.
        // -------------------------------------------------------------
        Vector3 posColumna = new Vector3(0f, 1f, 0f);
        GameObject goColumna = new GameObject("2_Columna_Giro");
        Undo.RegisterCreatedObjectUndo(goColumna, "Crear 2_Columna_Giro");
        goColumna.transform.SetParent(goBase.transform, false);
        goColumna.transform.localPosition = posColumna;
        goColumna.transform.localRotation = Quaternion.identity;
        goColumna.transform.localScale = Vector3.one;

        BoxCollider colColumna = goColumna.AddComponent<BoxCollider>();
        colColumna.center = new Vector3(0f, 2f, 0f);
        colColumna.size = new Vector3(0.6f, 4f, 0.6f);

        abColumna = goColumna.AddComponent<ArticulationBody>();
        abColumna.jointType = ArticulationJointType.RevoluteJoint;
        abColumna.mass = 150f;
        abColumna.solverIterations = 16;
        abColumna.solverVelocityIterations = 8;

        Quaternion rotGiroY = Quaternion.Euler(0f, 0f, 90f);
        abColumna.matchAnchors = false;
        abColumna.anchorPosition = Vector3.zero;
        abColumna.parentAnchorPosition = posColumna;
        abColumna.anchorRotation = rotGiroY;
        abColumna.parentAnchorRotation = rotGiroY;
        abColumna.twistLock = ArticulationDofLock.LimitedMotion;

        ArticulationDrive driveColumna = abColumna.xDrive;
        driveColumna.stiffness = 50000f;
        driveColumna.damping = 5000f;
        driveColumna.forceLimit = 20000f;
        driveColumna.lowerLimit = -180f;
        driveColumna.upperLimit = 180f;
        driveColumna.target = 0f;
        abColumna.xDrive = driveColumna;

        GameObject visualColumna = new GameObject("Visual");
        visualColumna.transform.SetParent(goColumna.transform, false);
        CrearVisualCubo(visualColumna.transform, "MastilTorre", new Vector3(0f, 2f, 0f), new Vector3(0.6f, 4f, 0.6f), matAmarillo);
        CrearVisualCubo(visualColumna.transform, "CoronaGiro", new Vector3(0f, 0.08f, 0f), new Vector3(1.1f, 0.16f, 1.1f), matChasis);
        CrearVisualCubo(visualColumna.transform, "CabinaCuerpo", new Vector3(0.55f, 3.4f, 0.35f), new Vector3(0.55f, 0.75f, 0.8f), matChasis);
        CrearVisualCubo(visualColumna.transform, "CabinaVidrio", new Vector3(0.55f, 3.55f, 0.76f), new Vector3(0.48f, 0.45f, 0.04f), matCabina);

        GameObject goCamCabina = new GameObject("Camara_CabinaOperador");
        goCamCabina.transform.SetParent(goColumna.transform, false);
        goCamCabina.transform.localPosition = new Vector3(0.55f, 3.65f, 0.65f);
        goCamCabina.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
        camaraCabina = goCamCabina.AddComponent<Camera>();
        camaraCabina.fieldOfView = 65f;
        camaraCabina.nearClipPlane = 0.1f;
        camaraCabina.enabled = false;

        // -------------------------------------------------------------
        // 3. 3_Pluma_Brazo (Hijo de 2): Pos local (0, 4, 0), Joint Fixed.
        //    Mass=80. BoxCollider (0.5, 0.5, 6) en Z=2.5.
        // -------------------------------------------------------------
        Vector3 posPluma = new Vector3(0f, 4f, 0f);
        GameObject goPluma = new GameObject("3_Pluma_Brazo");
        Undo.RegisterCreatedObjectUndo(goPluma, "Crear 3_Pluma_Brazo");
        goPluma.transform.SetParent(goColumna.transform, false);
        goPluma.transform.localPosition = posPluma;
        goPluma.transform.localRotation = Quaternion.identity;
        goPluma.transform.localScale = Vector3.one;

        BoxCollider colPluma = goPluma.AddComponent<BoxCollider>();
        colPluma.center = new Vector3(0f, 0f, 2.5f);
        colPluma.size = new Vector3(0.5f, 0.5f, 6f);

        abPluma = goPluma.AddComponent<ArticulationBody>();
        abPluma.jointType = ArticulationJointType.FixedJoint;
        abPluma.mass = 80f;
        abPluma.solverIterations = 16;
        abPluma.solverVelocityIterations = 8;
        abPluma.matchAnchors = false;
        abPluma.anchorPosition = Vector3.zero;
        abPluma.parentAnchorPosition = posPluma;
        abPluma.anchorRotation = Quaternion.identity;
        abPluma.parentAnchorRotation = Quaternion.identity;

        GameObject visualPluma = new GameObject("Visual");
        visualPluma.transform.SetParent(goPluma.transform, false);
        CrearVisualCubo(visualPluma.transform, "BrazoFrontal", new Vector3(0f, 0f, 2.5f), new Vector3(0.5f, 0.5f, 6f), matAmarillo);
        CrearVisualCubo(visualPluma.transform, "ContraplumaTrasera", new Vector3(0f, 0f, -1.2f), new Vector3(0.42f, 0.42f, 2.4f), matChasis);
        CrearVisualCubo(visualPluma.transform, "ContrapesoHormigon", new Vector3(0f, -0.1f, -2.1f), new Vector3(0.95f, 0.65f, 0.85f), matContrapeso);
        CrearVisualCubo(visualPluma.transform, "CuspideSuperior", new Vector3(0f, 0.65f, 0f), new Vector3(0.35f, 0.8f, 0.35f), matAmarillo);

        // -------------------------------------------------------------
        // 4. 4_Carro (Hijo de 3): Pos local (0, -0.1, 3), Joint Prismatic en Z.
        //    Mass=30. BoxCollider (0.7, 0.3, 0.7).
        // -------------------------------------------------------------
        Vector3 posCarro = new Vector3(0f, -0.1f, 3f);
        GameObject goCarro = new GameObject("4_Carro");
        Undo.RegisterCreatedObjectUndo(goCarro, "Crear 4_Carro");
        goCarro.transform.SetParent(goPluma.transform, false);
        goCarro.transform.localPosition = posCarro;
        goCarro.transform.localRotation = Quaternion.identity;
        goCarro.transform.localScale = Vector3.one;

        BoxCollider colCarro = goCarro.AddComponent<BoxCollider>();
        colCarro.center = Vector3.zero;
        colCarro.size = new Vector3(0.7f, 0.3f, 0.7f);

        abCarro = goCarro.AddComponent<ArticulationBody>();
        abCarro.jointType = ArticulationJointType.PrismaticJoint;
        abCarro.mass = 30f;
        abCarro.solverIterations = 16;
        abCarro.solverVelocityIterations = 8;

        Quaternion rotPrismaticZ = Quaternion.Euler(0f, -90f, 0f);
        abCarro.matchAnchors = false;
        abCarro.anchorPosition = Vector3.zero;
        abCarro.parentAnchorPosition = posCarro;
        abCarro.anchorRotation = rotPrismaticZ;
        abCarro.parentAnchorRotation = rotPrismaticZ;
        abCarro.linearLockX = ArticulationDofLock.LimitedMotion;

        ArticulationDrive driveCarro = abCarro.xDrive;
        driveCarro.stiffness = 30000f;
        driveCarro.damping = 3000f;
        driveCarro.forceLimit = 10000f;
        driveCarro.lowerLimit = -1.5f;
        driveCarro.upperLimit = 2.0f;
        driveCarro.target = 0f;
        abCarro.xDrive = driveCarro;

        GameObject visualCarro = new GameObject("Visual");
        visualCarro.transform.SetParent(goCarro.transform, false);
        CrearVisualCubo(visualCarro.transform, "CuerpoCarro", Vector3.zero, new Vector3(0.7f, 0.3f, 0.7f), matChasis);
        CrearVisualCubo(visualCarro.transform, "GuiaIzq", new Vector3(-0.28f, 0.2f, 0f), new Vector3(0.12f, 0.15f, 0.65f), matAmarillo);
        CrearVisualCubo(visualCarro.transform, "GuiaDer", new Vector3(0.28f, 0.2f, 0f), new Vector3(0.12f, 0.15f, 0.65f), matAmarillo);

        GameObject goCamGancho = new GameObject("Camara_CenitalGancho");
        goCamGancho.transform.SetParent(goCarro.transform, false);
        goCamGancho.transform.localPosition = new Vector3(0f, -0.3f, 0f);
        goCamGancho.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        camaraGancho = goCamGancho.AddComponent<Camera>();
        camaraGancho.fieldOfView = 60f;
        camaraGancho.nearClipPlane = 0.1f;
        camaraGancho.enabled = false;

        // -------------------------------------------------------------
        // 5. 5_Cable_Izaje (Hijo de 4): Pos local (0, -0.5, 0), Joint Prismatic en Y.
        //    Mass=15. BoxCollider (0.15, 2, 0.15) en Y=-1.
        // -------------------------------------------------------------
        Vector3 posCable = new Vector3(0f, -0.5f, 0f);
        GameObject goCable = new GameObject("5_Cable_Izaje");
        Undo.RegisterCreatedObjectUndo(goCable, "Crear 5_Cable_Izaje");
        goCable.transform.SetParent(goCarro.transform, false);
        goCable.transform.localPosition = posCable;
        goCable.transform.localRotation = Quaternion.identity;
        goCable.transform.localScale = Vector3.one;

        BoxCollider colCable = goCable.AddComponent<BoxCollider>();
        colCable.center = new Vector3(0f, -1f, 0f);
        colCable.size = new Vector3(0.15f, 2f, 0.15f);

        abCable = goCable.AddComponent<ArticulationBody>();
        abCable.jointType = ArticulationJointType.PrismaticJoint;
        abCable.mass = 15f;
        abCable.solverIterations = 16;
        abCable.solverVelocityIterations = 8;

        Quaternion rotPrismaticY = Quaternion.Euler(0f, 0f, 90f);
        abCable.matchAnchors = false;
        abCable.anchorPosition = Vector3.zero;
        abCable.parentAnchorPosition = posCable;
        abCable.anchorRotation = rotPrismaticY;
        abCable.parentAnchorRotation = rotPrismaticY;
        abCable.linearLockX = ArticulationDofLock.LimitedMotion;

        ArticulationDrive driveCable = abCable.xDrive;
        driveCable.stiffness = 30000f;
        driveCable.damping = 3000f;
        driveCable.forceLimit = 10000f;
        driveCable.lowerLimit = -1.5f;
        driveCable.upperLimit = 1.0f;
        driveCable.target = 0f;
        abCable.xDrive = driveCable;

        GameObject visualCable = new GameObject("Visual");
        visualCable.transform.SetParent(goCable.transform, false);
        CrearVisualCubo(visualCable.transform, "TiraCable", new Vector3(0f, -1f, 0f), new Vector3(0.15f, 2f, 0.15f), matCable);

        // -------------------------------------------------------------
        // 6. 6_Cabezal_Pendulo (Hijo de 5): Pos local (0, -2.1, 0), Joint Spherical PASIVO.
        //    Mass=10. BoxCollider (0.8, 0.2, 0.4).
        //    NUEVO DISEÑO DE GARRA ROBÓTICA: Cabezal en Azul Cobalto con cilindro hidráulico central
        //    y poleas laterales de reenvío.
        // -------------------------------------------------------------
        Vector3 posCabezal = new Vector3(0f, -2.1f, 0f);
        GameObject goCabezal = new GameObject("6_Cabezal_Pendulo");
        Undo.RegisterCreatedObjectUndo(goCabezal, "Crear 6_Cabezal_Pendulo");
        goCabezal.transform.SetParent(goCable.transform, false);
        goCabezal.transform.localPosition = posCabezal;
        goCabezal.transform.localRotation = Quaternion.identity;
        goCabezal.transform.localScale = Vector3.one;

        BoxCollider colCabezal = goCabezal.AddComponent<BoxCollider>();
        colCabezal.center = Vector3.zero;
        colCabezal.size = new Vector3(0.8f, 0.2f, 0.4f);

        abCabezal = goCabezal.AddComponent<ArticulationBody>();
        abCabezal.jointType = ArticulationJointType.SphericalJoint;
        abCabezal.mass = 10f;
        abCabezal.solverIterations = 16;
        abCabezal.solverVelocityIterations = 8;

        Quaternion rotSpherical = Quaternion.Euler(0f, 0f, 90f);
        abCabezal.matchAnchors = false;
        abCabezal.anchorPosition = Vector3.zero;
        abCabezal.parentAnchorPosition = posCabezal;
        abCabezal.anchorRotation = rotSpherical;
        abCabezal.parentAnchorRotation = rotSpherical;

        abCabezal.twistLock = ArticulationDofLock.LockedMotion;
        abCabezal.swingYLock = ArticulationDofLock.LimitedMotion;
        abCabezal.swingZLock = ArticulationDofLock.LimitedMotion;

        ArticulationDrive driveSwingY = abCabezal.yDrive;
        driveSwingY.stiffness = 0f;
        driveSwingY.damping = 15f;
        driveSwingY.forceLimit = 500f;
        driveSwingY.lowerLimit = -20f;
        driveSwingY.upperLimit = 20f;
        abCabezal.yDrive = driveSwingY;

        ArticulationDrive driveSwingZ = abCabezal.zDrive;
        driveSwingZ.stiffness = 0f;
        driveSwingZ.damping = 15f;
        driveSwingZ.forceLimit = 500f;
        driveSwingZ.lowerLimit = -20f;
        driveSwingZ.upperLimit = 20f;
        abCabezal.zDrive = driveSwingZ;

        ArticulationDrive driveTwist = abCabezal.xDrive;
        driveTwist.stiffness = 0f;
        driveTwist.damping = 15f;
        abCabezal.xDrive = driveTwist;

        // Visual Avanzado del Cabezal de la Garra Robótica
        GameObject visualCabezal = new GameObject("Visual");
        visualCabezal.transform.SetParent(goCabezal.transform, false);

        // Bloque central en azul cobalto
        CrearVisualCubo(visualCabezal.transform, "ChasisCabezal", Vector3.zero, new Vector3(0.8f, 0.2f, 0.4f), matGarraCuerpo);
        // Franja de advertencia amarilla
        CrearVisualCubo(visualCabezal.transform, "FranjaAdvertencia", new Vector3(0f, 0f, 0.205f), new Vector3(0.7f, 0.08f, 0.01f), matGarraDetalle);
        // Perno de conexión superior cromado
        CrearVisualCubo(visualCabezal.transform, "RotulaSuperior", new Vector3(0f, 0.12f, 0f), new Vector3(0.24f, 0.08f, 0.24f), matGarraCromo);
        // Cilindro hidráulico central horizontal
        CrearVisualCubo(visualCabezal.transform, "CilindroHidraulico", new Vector3(0f, 0.02f, 0f), new Vector3(0.68f, 0.10f, 0.14f), matGarraCromo);
        // Tapas laterales del actuador en amarillo
        CrearVisualCubo(visualCabezal.transform, "TapaActuadorIzq", new Vector3(-0.38f, 0.02f, 0f), new Vector3(0.06f, 0.14f, 0.18f), matGarraDetalle);
        CrearVisualCubo(visualCabezal.transform, "TapaActuadorDer", new Vector3(0.38f, 0.02f, 0f), new Vector3(0.06f, 0.14f, 0.18f), matGarraDetalle);

        // -------------------------------------------------------------
        // 7a y 7b. Dedos de la Pinza: Pos locales (-0.4, -0.5, 0) y (0.4, -0.5, 0)
        //    Joint Prismatic en eje X. ContinuousDynamic. Mass=3. BoxCollider (0.1, 0.6, 0.4).
        //    NUEVO DISEÑO DE GARRA: Mandíbulas en Azul Cobalto, almohadillas vulcanizadas de agarre
        //    y uñas inferiores de apoyo en amarillo de advertencia.
        // -------------------------------------------------------------
        // Dedo Izquierdo
        Vector3 posDedoIzq = new Vector3(-0.4f, -0.5f, 0f);
        GameObject goDedoIzq = new GameObject("7a_Dedo_Izq");
        Undo.RegisterCreatedObjectUndo(goDedoIzq, "Crear 7a_Dedo_Izq");
        goDedoIzq.transform.SetParent(goCabezal.transform, false);
        goDedoIzq.transform.localPosition = posDedoIzq;
        goDedoIzq.transform.localRotation = Quaternion.identity;
        goDedoIzq.transform.localScale = Vector3.one;

        BoxCollider colIzq = goDedoIzq.AddComponent<BoxCollider>();
        colIzq.center = Vector3.zero;
        colIzq.size = new Vector3(0.1f, 0.6f, 0.4f);
        colIzq.sharedMaterial = physMatPinza;

        abDedoIzq = goDedoIzq.AddComponent<ArticulationBody>();
        abDedoIzq.jointType = ArticulationJointType.PrismaticJoint;
        abDedoIzq.mass = 3f;
        abDedoIzq.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        abDedoIzq.solverIterations = 16;
        abDedoIzq.solverVelocityIterations = 8;
        abDedoIzq.matchAnchors = false;
        abDedoIzq.anchorPosition = Vector3.zero;
        abDedoIzq.parentAnchorPosition = posDedoIzq;
        abDedoIzq.anchorRotation = Quaternion.identity;
        abDedoIzq.parentAnchorRotation = Quaternion.identity;
        abDedoIzq.linearLockX = ArticulationDofLock.LimitedMotion;

        ArticulationDrive driveIzq = abDedoIzq.xDrive;
        driveIzq.stiffness = 15000f;
        driveIzq.damping = 1500f;
        driveIzq.forceLimit = 800f;
        driveIzq.lowerLimit = 0f;
        driveIzq.upperLimit = 0.25f;
        driveIzq.target = 0f;
        abDedoIzq.xDrive = driveIzq;

        GameObject visualDedoIzq = new GameObject("Visual");
        visualDedoIzq.transform.SetParent(goDedoIzq.transform, false);
        // Brazo vertical de la mandíbula izquierda en Azul Cobalto
        CrearVisualCubo(visualDedoIzq.transform, "MandibulaIzq", Vector3.zero, new Vector3(0.1f, 0.6f, 0.4f), matGarraCuerpo);
        // Almohadilla interna de goma vulcanizada de alto agarre
        CrearVisualCubo(visualDedoIzq.transform, "GripGomaIzq", new Vector3(0.045f, 0f, 0f), new Vector3(0.02f, 0.5f, 0.36f), matGarraGoma);
        // Uña inferior de apoyo (extendiéndose hacia adentro en X positivo) en Amarillo de Advertencia
        CrearVisualCubo(visualDedoIzq.transform, "UnaApoyoIzq", new Vector3(0.07f, -0.27f, 0f), new Vector3(0.08f, 0.06f, 0.38f), matGarraDetalle);
        // Vástago de pistón hidráulico cromado en la parte superior
        CrearVisualCubo(visualDedoIzq.transform, "VastagoPistonIzq", new Vector3(0f, 0.25f, 0f), new Vector3(0.08f, 0.15f, 0.12f), matGarraCromo);

        // Dedo Derecho
        Vector3 posDedoDer = new Vector3(0.4f, -0.5f, 0f);
        GameObject goDedoDer = new GameObject("7b_Dedo_Der");
        Undo.RegisterCreatedObjectUndo(goDedoDer, "Crear 7b_Dedo_Der");
        goDedoDer.transform.SetParent(goCabezal.transform, false);
        goDedoDer.transform.localPosition = posDedoDer;
        goDedoDer.transform.localRotation = Quaternion.identity;
        goDedoDer.transform.localScale = Vector3.one;

        BoxCollider colDer = goDedoDer.AddComponent<BoxCollider>();
        colDer.center = Vector3.zero;
        colDer.size = new Vector3(0.1f, 0.6f, 0.4f);
        colDer.sharedMaterial = physMatPinza;

        abDedoDer = goDedoDer.AddComponent<ArticulationBody>();
        abDedoDer.jointType = ArticulationJointType.PrismaticJoint;
        abDedoDer.mass = 3f;
        abDedoDer.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        abDedoDer.solverIterations = 16;
        abDedoDer.solverVelocityIterations = 8;
        abDedoDer.matchAnchors = false;
        abDedoDer.anchorPosition = Vector3.zero;
        abDedoDer.parentAnchorPosition = posDedoDer;
        abDedoDer.anchorRotation = Quaternion.identity;
        abDedoDer.parentAnchorRotation = Quaternion.identity;
        abDedoDer.linearLockX = ArticulationDofLock.LimitedMotion;

        ArticulationDrive driveDer = abDedoDer.xDrive;
        driveDer.stiffness = 15000f;
        driveDer.damping = 1500f;
        driveDer.forceLimit = 800f;
        driveDer.lowerLimit = -0.25f;
        driveDer.upperLimit = 0f;
        driveDer.target = 0f;
        abDedoDer.xDrive = driveDer;

        GameObject visualDedoDer = new GameObject("Visual");
        visualDedoDer.transform.SetParent(goDedoDer.transform, false);
        // Brazo vertical de la mandíbula derecha en Azul Cobalto
        CrearVisualCubo(visualDedoDer.transform, "MandibulaDer", Vector3.zero, new Vector3(0.1f, 0.6f, 0.4f), matGarraCuerpo);
        // Almohadilla interna de goma vulcanizada de alto agarre
        CrearVisualCubo(visualDedoDer.transform, "GripGomaDer", new Vector3(-0.045f, 0f, 0f), new Vector3(0.02f, 0.5f, 0.36f), matGarraGoma);
        // Uña inferior de apoyo (extendiéndose hacia adentro en X negativo) en Amarillo de Advertencia
        CrearVisualCubo(visualDedoDer.transform, "UnaApoyoDer", new Vector3(-0.07f, -0.27f, 0f), new Vector3(0.08f, 0.06f, 0.38f), matGarraDetalle);
        // Vástago de pistón hidráulico cromado en la parte superior
        CrearVisualCubo(visualDedoDer.transform, "VastagoPistonDer", new Vector3(0f, 0.25f, 0f), new Vector3(0.08f, 0.15f, 0.12f), matGarraCromo);

        return goBase;
    }

    private static GameObject CrearVisualCubo(Transform parent, string nombre, Vector3 localPos, Vector3 localScale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nombre;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = localScale;

        Collider c = go.GetComponent<Collider>();
        if (c != null) Object.DestroyImmediate(c);

        if (mat != null)
        {
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;
        }

        return go;
    }

    /// <summary>
    /// Construye el suelo con demarcación de seguridad amarilla/negra alrededor de la base.
    /// </summary>
    private static GameObject ConstruirPiso(Material matPiso, Material matPeligro, PhysicsMaterial physMat)
    {
        GameObject goEntorno = GameObject.Find("Entorno_Grua");
        if (goEntorno == null)
        {
            goEntorno = new GameObject("Entorno_Grua");
            Undo.RegisterCreatedObjectUndo(goEntorno, "Crear Entorno");
        }

        GameObject piso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        piso.name = "Piso";
        piso.transform.SetParent(goEntorno.transform, false);
        piso.transform.position = new Vector3(0f, -0.5f, 0f);
        piso.transform.localScale = new Vector3(30f, 1f, 30f);

        BoxCollider col = piso.GetComponent<BoxCollider>();
        if (col != null && physMat != null) col.sharedMaterial = physMat;

        MeshRenderer mr = piso.GetComponent<MeshRenderer>();
        if (mr != null && matPiso != null) mr.sharedMaterial = matPiso;

        GameObject franjaPeligro = GameObject.CreatePrimitive(PrimitiveType.Cube);
        franjaPeligro.name = "ZonaSeguridadBase";
        franjaPeligro.transform.SetParent(goEntorno.transform, false);
        franjaPeligro.transform.position = new Vector3(0f, 0.005f, 0f);
        franjaPeligro.transform.localScale = new Vector3(3.2f, 0.01f, 3.2f);
        Collider cp = franjaPeligro.GetComponent<Collider>();
        if (cp != null) Object.DestroyImmediate(cp);
        MeshRenderer mrp = franjaPeligro.GetComponent<MeshRenderer>();
        if (mrp != null && matPeligro != null) mrp.sharedMaterial = matPeligro;

        return piso;
    }

    /// <summary>
    /// Construye una Mesa de Trabajo Industrial en (3, 0, 0) en lugar de una plataforma simple.
    /// Incluye tablero de acero galvanizado con BoxCollider y PhysicsMaterial de alta fricción,
    /// 4 patas robustas de acero con bases de apoyo, largueros de refuerzo y barandilla posterior.
    /// </summary>
    private static GameObject ConstruirMesaTrabajo(Material matTablero, Material matEstructura, Material matBarandilla, PhysicsMaterial physMat)
    {
        GameObject goEntorno = GameObject.Find("Entorno_Grua");
        if (goEntorno == null)
        {
            goEntorno = new GameObject("Entorno_Grua");
            Undo.RegisterCreatedObjectUndo(goEntorno, "Crear Entorno");
        }

        // Raíz de la mesa ubicada a radio 3m de la grúa
        GameObject mesa = new GameObject("Mesa_Trabajo_Destino");
        mesa.transform.SetParent(goEntorno.transform, false);
        mesa.transform.position = new Vector3(3f, 0.59f, 0f);

        // 1. Tablero principal con BoxCollider físico para recibir las cargas
        BoxCollider colTablero = mesa.AddComponent<BoxCollider>();
        colTablero.center = Vector3.zero;
        colTablero.size = new Vector3(2.2f, 0.12f, 2.0f);
        if (physMat != null) colTablero.sharedMaterial = physMat;

        // Visual del Tablero de Acero
        GameObject visualTablero = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visualTablero.name = "TableroAcero";
        visualTablero.transform.SetParent(mesa.transform, false);
        visualTablero.transform.localPosition = Vector3.zero;
        visualTablero.transform.localScale = new Vector3(2.2f, 0.12f, 2.0f);
        Collider ct = visualTablero.GetComponent<Collider>();
        if (ct != null) Object.DestroyImmediate(ct);
        MeshRenderer mrt = visualTablero.GetComponent<MeshRenderer>();
        if (mrt != null && matTablero != null) mrt.sharedMaterial = matTablero;

        // 2. Barandilla de seguridad posterior en el borde exterior (X positivo)
        GameObject barandilla = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barandilla.name = "BarandillaRetencion";
        barandilla.transform.SetParent(mesa.transform, false);
        barandilla.transform.localPosition = new Vector3(1.05f, 0.12f, 0f);
        barandilla.transform.localScale = new Vector3(0.08f, 0.16f, 2.0f);
        BoxCollider cb = barandilla.GetComponent<BoxCollider>();
        if (cb != null && physMat != null) cb.sharedMaterial = physMat;
        MeshRenderer mrb = barandilla.GetComponent<MeshRenderer>();
        if (mrb != null && matBarandilla != null) mrb.sharedMaterial = matBarandilla;

        // 3. Patas robustas de soporte (4 patas de acero)
        Vector3[] posicionesPatas = new Vector3[]
        {
            new Vector3(-0.95f, -0.325f, -0.85f),
            new Vector3(0.95f, -0.325f, -0.85f),
            new Vector3(-0.95f, -0.325f, 0.85f),
            new Vector3(0.95f, -0.325f, 0.85f)
        };

        for (int i = 0; i < posicionesPatas.Length; i++)
        {
            GameObject pata = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pata.name = $"PataMesa_{i + 1}";
            pata.transform.SetParent(mesa.transform, false);
            pata.transform.localPosition = posicionesPatas[i];
            pata.transform.localScale = new Vector3(0.14f, 0.53f, 0.14f);
            BoxCollider cp = pata.GetComponent<BoxCollider>();
            if (cp != null && physMat != null) cp.sharedMaterial = physMat;
            MeshRenderer mrp = pata.GetComponent<MeshRenderer>();
            if (mrp != null && matEstructura != null) mrp.sharedMaterial = matEstructura;
        }

        // 4. Largueros estructurales inferiores de refuerzo
        GameObject larguero1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        larguero1.name = "LargueroLongitudinal_1";
        larguero1.transform.SetParent(mesa.transform, false);
        larguero1.transform.localPosition = new Vector3(0f, -0.35f, -0.85f);
        larguero1.transform.localScale = new Vector3(1.9f, 0.08f, 0.08f);
        Collider cl1 = larguero1.GetComponent<Collider>();
        if (cl1 != null) Object.DestroyImmediate(cl1);
        MeshRenderer mrl1 = larguero1.GetComponent<MeshRenderer>();
        if (mrl1 != null && matEstructura != null) mrl1.sharedMaterial = matEstructura;

        GameObject larguero2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        larguero2.name = "LargueroLongitudinal_2";
        larguero2.transform.SetParent(mesa.transform, false);
        larguero2.transform.localPosition = new Vector3(0f, -0.35f, 0.85f);
        larguero2.transform.localScale = new Vector3(1.9f, 0.08f, 0.08f);
        Collider cl2 = larguero2.GetComponent<Collider>();
        if (cl2 != null) Object.DestroyImmediate(cl2);
        MeshRenderer mrl2 = larguero2.GetComponent<MeshRenderer>();
        if (mrl2 != null && matEstructura != null) mrl2.sharedMaterial = matEstructura;

        return mesa;
    }

    /// <summary>
    /// Crea 3 Cajas de Carga personalizadas con colores de contenedor marítimo/industrial.
    /// </summary>
    private static Rigidbody[] ConstruirCajasCarga(Material matCaja1, Material matCaja2, Material matCaja3, PhysicsMaterial physMat)
    {
        GameObject goEntorno = GameObject.Find("Entorno_Grua");
        if (goEntorno == null)
        {
            goEntorno = new GameObject("Entorno_Grua");
            Undo.RegisterCreatedObjectUndo(goEntorno, "Crear Entorno");
        }

        GameObject containerCajas = new GameObject("Cajas_Carga");
        containerCajas.transform.SetParent(goEntorno.transform, false);

        Vector3[] posiciones = new Vector3[]
        {
            new Vector3(0f, 0.26f, 3.0f),
            new Vector3(0.75f, 0.26f, 3.0f),
            new Vector3(-0.75f, 0.26f, 3.0f)
        };

        Material[] materiales = new Material[] { matCaja1, matCaja2, matCaja3 };
        string[] nombres = new string[] { "Caja_Carga_1 (Rojo)", "Caja_Carga_2 (Azul)", "Caja_Carga_3 (Madera)" };

        Rigidbody[] rbs = new Rigidbody[posiciones.Length];

        for (int i = 0; i < posiciones.Length; i++)
        {
            GameObject caja = GameObject.CreatePrimitive(PrimitiveType.Cube);
            caja.name = nombres[i];
            caja.transform.SetParent(containerCajas.transform, false);
            caja.transform.position = posiciones[i];
            caja.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            BoxCollider col = caja.GetComponent<BoxCollider>();
            if (col != null) col.sharedMaterial = physMat;

            MeshRenderer mr = caja.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = materiales[i];

            Rigidbody rb = caja.AddComponent<Rigidbody>();
            rb.mass = 2f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            rbs[i] = rb;
        }

        return rbs;
    }

    /// <summary>
    /// Construye el Canvas y panel de telemetría de diseño industrial moderno.
    /// </summary>
    private static void ConstruirUI(
        out Slider sliderGiro,
        out Slider sliderCarro,
        out Slider sliderCable,
        out Slider sliderPinza,
        out Text textGiro,
        out Text textCarro,
        out Text textCable,
        out Text textPinza,
        out Text textPendulo,
        out Text textMision,
        out Text textModoCamara,
        out Button botonCambiarCamara,
        out Button botonCentrar,
        out Button botonModoSuave,
        out Button botonReiniciar)
    {
        Font font = ObtenerFuentePorDefecto();

        Sprite standardSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Sprite backgroundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        Sprite knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        // 1. Canvas
        GameObject canvasGO = new GameObject("Canvas_GruaUI");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Crear Canvas UI");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // 2. EventSystem configurado con New Input System
        GameObject esGO = new GameObject("EventSystem");
        Undo.RegisterCreatedObjectUndo(esGO, "Crear EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();

#if ENABLE_INPUT_SYSTEM
        var inputModule = esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        var actionsAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        if (actionsAsset != null)
        {
            inputModule.actionsAsset = actionsAsset;
            var subAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/InputSystem_Actions.inputactions");
            foreach (var sub in subAssets)
            {
                if (sub is UnityEngine.InputSystem.InputActionReference reference && reference.action != null)
                {
                    switch (reference.action.name)
                    {
                        case "Point": inputModule.point = reference; break;
                        case "Click": inputModule.leftClick = reference; break;
                        case "RightClick": inputModule.rightClick = reference; break;
                        case "MiddleClick": inputModule.middleClick = reference; break;
                        case "ScrollWheel": inputModule.scrollWheel = reference; break;
                        case "Navigate": inputModule.move = reference; break;
                        case "Submit": inputModule.submit = reference; break;
                        case "Cancel": inputModule.cancel = reference; break;
                    }
                }
            }
        }
        else
        {
            inputModule.AssignDefaultActions();
        }
#else
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif

        // 3. Panel Lateral
        GameObject panelGO = new GameObject("Panel_Lateral");
        panelGO.transform.SetParent(canvasGO.transform, false);

        RectTransform rtPanel = panelGO.AddComponent<RectTransform>();
        rtPanel.anchorMin = new Vector2(0f, 0f);
        rtPanel.anchorMax = new Vector2(0f, 1f);
        rtPanel.pivot = new Vector2(0f, 0.5f);
        rtPanel.offsetMin = new Vector2(20f, 20f);
        rtPanel.offsetMax = new Vector2(490f, -20f);

        Image imgPanel = panelGO.AddComponent<Image>();
        imgPanel.color = new Color(0.07f, 0.09f, 0.12f, 0.95f);
        imgPanel.raycastTarget = false;

        VerticalLayoutGroup vlg = panelGO.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 18, 18);
        vlg.spacing = 10;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        CrearHeaderUI(panelGO.transform, font);

        textMision = CrearBloqueMision(panelGO.transform, font);
        textPendulo = CrearBloqueTelemetria(panelGO.transform, font);

        sliderGiro = CrearControlSlider(panelGO.transform, "🔄 Columna Giro", -180f, 180f, 0f, font, backgroundSprite, standardSprite, knobSprite, out textGiro);
        sliderCarro = CrearControlSlider(panelGO.transform, "↔️ Carro Guía", -1.5f, 2.0f, 0f, font, backgroundSprite, standardSprite, knobSprite, out textCarro);
        sliderCable = CrearControlSlider(panelGO.transform, "↕️ Cable Izaje", -1.5f, 1.0f, 0f, font, backgroundSprite, standardSprite, knobSprite, out textCable);
        sliderPinza = CrearControlSlider(panelGO.transform, "🤏 Garra Robótica", 0f, 0.25f, 0f, font, backgroundSprite, standardSprite, knobSprite, out textPinza);

        GameObject filaBotones1 = new GameObject("Fila_BotonesAccion_1");
        filaBotones1.transform.SetParent(panelGO.transform, false);
        LayoutElement leFila1 = filaBotones1.AddComponent<LayoutElement>();
        leFila1.preferredHeight = 42;

        HorizontalLayoutGroup hlg1 = filaBotones1.AddComponent<HorizontalLayoutGroup>();
        hlg1.spacing = 8;
        hlg1.childControlWidth = true;
        hlg1.childControlHeight = true;
        hlg1.childForceExpandWidth = true;
        hlg1.childForceExpandHeight = true;

        botonCambiarCamara = CrearBotonUI(filaBotones1.transform, "Btn_Camara", "🎥 Cámara [V]", font, standardSprite, new Color(0.2f, 0.45f, 0.75f));
        botonCentrar = CrearBotonUI(filaBotones1.transform, "Btn_Centrar", "🎯 Centrar", font, standardSprite, new Color(0.25f, 0.35f, 0.45f));

        GameObject filaBotones2 = new GameObject("Fila_BotonesAccion_2");
        filaBotones2.transform.SetParent(panelGO.transform, false);
        LayoutElement leFila2 = filaBotones2.AddComponent<LayoutElement>();
        leFila2.preferredHeight = 42;

        HorizontalLayoutGroup hlg2 = filaBotones2.AddComponent<HorizontalLayoutGroup>();
        hlg2.spacing = 8;
        hlg2.childControlWidth = true;
        hlg2.childControlHeight = true;
        hlg2.childForceExpandWidth = true;
        hlg2.childForceExpandHeight = true;

        botonModoSuave = CrearBotonUI(filaBotones2.transform, "Btn_ModoSuave", "🌿 Modo: Suave [T]", font, standardSprite, new Color(0.15f, 0.55f, 0.45f));
        botonReiniciar = CrearBotonUI(filaBotones2.transform, "Btn_Reiniciar", "🔄 Reiniciar", font, standardSprite, new Color(0.85f, 0.25f, 0.2f));

        textModoCamara = CrearIndicadorCamara(panelGO.transform, font);

        CrearFooterTip(panelGO.transform, font);
    }

    private static void CrearHeaderUI(Transform parent, Font font)
    {
        GameObject headerGO = new GameObject("Header");
        headerGO.transform.SetParent(parent, false);
        LayoutElement le = headerGO.AddComponent<LayoutElement>();
        le.preferredHeight = 54;

        Text text = headerGO.AddComponent<Text>();
        text.font = font;
        text.fontSize = 20;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.98f, 0.78f, 0.15f);
        text.text = "GRÚA INDUSTRIAL & GARRA ROBÓTICA\n<size=12><color=#8FA0B5>Control Cinemático & ArticulationBody • Unity 6</color></size>";
        text.supportRichText = true;
    }

    private static Text CrearBloqueMision(Transform parent, Font font)
    {
        GameObject container = new GameObject("Container_Mision");
        container.transform.SetParent(parent, false);
        LayoutElement le = container.AddComponent<LayoutElement>();
        le.preferredHeight = 36;

        Text text = container.AddComponent<Text>();
        text.font = font;
        text.fontSize = 15;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        text.supportRichText = true;
        text.text = "🛠️ <b>Misión de Carga:</b> Cajas en Mesa: <color=#F8C826>0 / 3</color>";

        return text;
    }

    private static Text CrearBloqueTelemetria(Transform parent, Font font)
    {
        GameObject container = new GameObject("Container_Pendulo");
        container.transform.SetParent(parent, false);
        LayoutElement le = container.AddComponent<LayoutElement>();
        le.preferredHeight = 44;

        Text text = container.AddComponent<Text>();
        text.font = font;
        text.fontSize = 14;
        text.color = new Color(0.4f, 0.88f, 1f, 1f);
        text.alignment = TextAnchor.MiddleLeft;
        text.supportRichText = true;
        text.text = "⏱️ <b>Péndulo Inercial</b> (Spherical Pasivo)\nInclinación: <color=#50D0FF>0.0°</color> (SwingY: 0.0°, SwingZ: 0.0°)";

        return text;
    }

    private static Slider CrearControlSlider(
        Transform parent,
        string titulo,
        float min,
        float max,
        float defaultVal,
        Font font,
        Sprite bgSprite,
        Sprite fillSprite,
        Sprite knobSprite,
        out Text telemetriaText)
    {
        GameObject container = new GameObject($"Container_{titulo}");
        container.transform.SetParent(parent, false);
        LayoutElement le = container.AddComponent<LayoutElement>();
        le.preferredHeight = 90;

        VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 4;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        GameObject textGO = new GameObject($"Text_{titulo}");
        textGO.transform.SetParent(container.transform, false);
        LayoutElement leText = textGO.AddComponent<LayoutElement>();
        leText.preferredHeight = 44;

        telemetriaText = textGO.AddComponent<Text>();
        telemetriaText.font = font;
        telemetriaText.fontSize = 15;
        telemetriaText.color = Color.white;
        telemetriaText.alignment = TextAnchor.MiddleLeft;
        telemetriaText.supportRichText = true;
        telemetriaText.raycastTarget = false;
        telemetriaText.text = $"<b>{titulo}</b>\nTarget: <color=#F8C826>{defaultVal:F2}</color>   |   Real: <color=#66FF88>0.00</color>";

        DefaultControls.Resources res = new DefaultControls.Resources
        {
            background = bgSprite,
            standard = fillSprite,
            knob = knobSprite
        };

        GameObject sliderGO = DefaultControls.CreateSlider(res);
        sliderGO.name = $"Slider_{titulo}";
        sliderGO.transform.SetParent(container.transform, false);

        LayoutElement leSlider = sliderGO.AddComponent<LayoutElement>();
        leSlider.preferredHeight = 36;

        RectTransform sliderRT = sliderGO.GetComponent<RectTransform>();
        if (sliderRT != null)
        {
            sliderRT.sizeDelta = new Vector2(0f, 36f);
        }

        Slider slider = sliderGO.GetComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = defaultVal;

        Transform bgTransform = sliderGO.transform.Find("Background");
        if (bgTransform != null)
        {
            Image bgImg = bgTransform.GetComponent<Image>();
            if (bgImg != null)
            {
                bgImg.color = new Color(0.18f, 0.22f, 0.28f, 1f);
                bgImg.raycastTarget = true;
            }
        }

        Transform fillTransform = sliderGO.transform.Find("Fill Area/Fill");
        if (fillTransform != null)
        {
            Image fillImg = fillTransform.GetComponent<Image>();
            if (fillImg != null)
            {
                fillImg.color = new Color(0.96f, 0.72f, 0.15f, 1f);
                fillImg.raycastTarget = true;
            }
        }

        Transform handleTransform = sliderGO.transform.Find("Handle Slide Area/Handle");
        if (handleTransform != null)
        {
            RectTransform handleRT = handleTransform.GetComponent<RectTransform>();
            if (handleRT != null)
            {
                handleRT.sizeDelta = new Vector2(36f, 0f);
            }

            Image handleImg = handleTransform.GetComponent<Image>();
            if (handleImg != null)
            {
                handleImg.color = Color.white;
                handleImg.raycastTarget = true;
            }
        }

        return slider;
    }

    private static Button CrearBotonUI(Transform parent, string id, string label, Font font, Sprite btnSprite, Color colorBase)
    {
        DefaultControls.Resources res = new DefaultControls.Resources
        {
            standard = btnSprite
        };

        GameObject btnGO = DefaultControls.CreateButton(res);
        btnGO.name = id;
        btnGO.transform.SetParent(parent, false);

        LayoutElement le = btnGO.AddComponent<LayoutElement>();
        le.preferredHeight = 44;

        Image img = btnGO.GetComponent<Image>();
        if (img != null) img.color = colorBase;

        Button btn = btnGO.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = colorBase;
        cb.highlightedColor = colorBase * 1.2f;
        cb.pressedColor = colorBase * 0.8f;
        btn.colors = cb;

        Text text = btnGO.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.font = font;
            text.fontSize = 15;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.text = label;
        }

        return btn;
    }

    private static Text CrearIndicadorCamara(Transform parent, Font font)
    {
        GameObject go = new GameObject("Text_ModoCamara");
        go.transform.SetParent(parent, false);
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 24;

        Text text = go.AddComponent<Text>();
        text.font = font;
        text.fontSize = 13;
        text.color = new Color(0.75f, 0.85f, 0.95f);
        text.alignment = TextAnchor.MiddleCenter;
        text.supportRichText = true;
        text.text = "🎥 Cámara: <b>[1/3] Vista Isométrica</b> <color=#90A4BE>(V)</color>";

        return text;
    }

    private static void CrearFooterTip(Transform parent, Font font)
    {
        GameObject tipGO = new GameObject("FooterTip");
        tipGO.transform.SetParent(parent, false);
        LayoutElement le = tipGO.AddComponent<LayoutElement>();
        le.preferredHeight = 52;

        Text text = tipGO.AddComponent<Text>();
        text.font = font;
        text.fontSize = 12;
        text.color = new Color(0.70f, 0.78f, 0.86f);
        text.alignment = TextAnchor.MiddleLeft;
        text.supportRichText = true;
        text.text = "<b>Mando:</b> Sliders o Teclado: <b>[A/D]</b> Giro • <b>[W/S]</b> Carro\n<b>[R/F]</b> Cable • <b>[C/Espacio]</b> Garra • <b>[T]</b> Modo Suave • <b>[V]</b> Cámara";
    }

    private static void ConfigurarIluminacion()
    {
        Light sol = Object.FindAnyObjectByType<Light>();
        if (sol == null)
        {
            GameObject solGO = new GameObject("Directional Light");
            sol = solGO.AddComponent<Light>();
            sol.type = LightType.Directional;
        }

        sol.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
        sol.color = new Color(1.0f, 0.97f, 0.92f);
        sol.intensity = 1.25f;
        sol.shadows = LightShadows.Soft;
    }

    private static Camera ConfigurarCamaraPrincipal()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            camGO.tag = "MainCamera";
        }

        cam.transform.position = new Vector3(6.8f, 6.2f, -2.8f);
        cam.transform.LookAt(new Vector3(1.2f, 2.2f, 1.5f));
        cam.fieldOfView = 60f;
        cam.enabled = true;

        return cam;
    }

    private static PhysicsMaterial ObtenerOCrearPhysicsMaterial()
    {
        string folder = "Assets/Settings";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets", "Settings");
        }

        string path = $"{folder}/FriccionAltaGrua.physicsMaterial";
        PhysicsMaterial mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);

        if (mat == null)
        {
            mat = new PhysicsMaterial("FriccionAltaGrua");
            mat.staticFriction = 1.0f;
            mat.dynamicFriction = 1.0f;
            mat.frictionCombine = PhysicsMaterialCombine.Maximum;
            mat.bounciness = 0.0f;
            mat.bounceCombine = PhysicsMaterialCombine.Minimum;
            AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();
        }
        else
        {
            mat.staticFriction = 1.0f;
            mat.dynamicFriction = 1.0f;
            mat.frictionCombine = PhysicsMaterialCombine.Maximum;
            EditorUtility.SetDirty(mat);
        }

        return mat;
    }

    private static Material ObtenerOCrearMaterial(string name, Color color, float smoothness = 0.3f, float metallic = 0.1f)
    {
        string folder = "Assets/Settings/Materials";
        if (!AssetDatabase.IsValidFolder("Assets/Settings"))
        {
            AssetDatabase.CreateFolder("Assets", "Settings");
        }
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets/Settings", "Materials");
        }

        string path = $"{folder}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = shader;
        }

        mat.color = color;
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color);
        }

        if (mat.HasProperty("_Smoothness"))
        {
            mat.SetFloat("_Smoothness", smoothness);
        }
        else if (mat.HasProperty("_Glossiness"))
        {
            mat.SetFloat("_Glossiness", smoothness);
        }

        if (mat.HasProperty("_Metallic"))
        {
            mat.SetFloat("_Metallic", metallic);
        }

        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Font ObtenerFuentePorDefecto()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        return font;
    }
}
