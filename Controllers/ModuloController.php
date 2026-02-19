<?php
require_once __DIR__ . '/../models/Modulo.php';
require_once __DIR__ . '/../../helpers/SessionHelper.php';
require_once __DIR__ . '/../../helpers/AuthHelper.php';
require_once __DIR__ . '/../../helpers/PermisoHelper.php';

class ModuloController {

    /**
     * Listar todos los módulos en árbol jerárquico
     */
    public function index() {
        PermisoHelper::requireSuperUsuario();
        AuthHelper::requireAuth();

        $moduloModel = new Modulo();
        $arbol = $moduloModel->getArbolCompletoConInactivos();

        require_once __DIR__ . '/../views/modulos/index.php';
    }

    /**
     * Mostrar formulario de creación
     * Acepta ?nivel=1|2|3 y ?padre_id=X como sugerencia
     */
    public function crear() {
        PermisoHelper::requireSuperUsuario();
        AuthHelper::requireAuth();

        $moduloModel = new Modulo();
        $secciones   = $moduloModel->getSecciones();
        $nivel2      = $moduloModel->getModulosNivel2();

        // Sugerencia de nivel y padre desde la URL
        $nivelSugerido = $_GET['nivel']    ?? 3;
        $padreSugerido = $_GET['padre_id'] ?? null;

        require_once __DIR__ . '/../views/modulos/crear.php';
    }

    /**
     * Guardar nuevo módulo (POST)
     */
    public function guardar() {
        PermisoHelper::requireSuperUsuario();
        AuthHelper::requireAuth();

        if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
            header('Location: /vetalmacen/public/index.php?url=modulos');
            exit();
        }

        $id          = trim($_POST['id']          ?? '');
        $idPadre     = $_POST['id_padre']          ?? null;
        $nivel       = $_POST['nivel']             ?? '';
        $nombre      = trim($_POST['nombre']       ?? '');
        $descripcion = trim($_POST['descripcion']  ?? '');
        $ruta        = trim($_POST['ruta']         ?? '');
        $icono       = trim($_POST['icono']        ?? '');
        $orden       = $_POST['orden']             ?? 0;

        // Validaciones
        $errores = [];

        if (empty($id) || !is_numeric($id)) {
            $errores[] = 'El ID es obligatorio y debe ser numérico';
        }

        if (empty($nombre)) {
            $errores[] = 'El nombre es obligatorio';
        }

        if (!in_array($nivel, ['1', '2', '3'])) {
            $errores[] = 'El nivel debe ser 1, 2 o 3';
        }

        if (in_array($nivel, ['2', '3']) && empty($idPadre)) {
            $errores[] = 'Debe seleccionar un módulo padre';
        }

        if (empty($ruta) && $nivel != '1') {
            $errores[] = 'La ruta es obligatoria para módulos y acciones';
        }

        // Verificar ID único
        $moduloModel = new Modulo();

        if ($moduloModel->getById($id)) {
            $errores[] = 'El ID ' . $id . ' ya existe en la base de datos';
        }

        // Verificar ruta única (solo si tiene ruta)
        if (!empty($ruta) && $moduloModel->rutaExiste($ruta)) {
            $errores[] = 'La ruta "' . $ruta . '" ya está registrada';
        }

        if (!empty($errores)) {
            SessionHelper::setFlash('danger', implode('<br>', $errores));
            header('Location: /vetalmacen/public/index.php?url=modulos/crear');
            exit();
        }

        // Crear módulo
        $moduloModel->Id          = $id;
        $moduloModel->IdPadre     = $nivel == '1' ? null : $idPadre;
        $moduloModel->Nivel       = $nivel;
        $moduloModel->Nombre      = $nombre;
        $moduloModel->Descripcion = $descripcion ?: null;
        $moduloModel->Ruta        = $ruta ?: null;
        $moduloModel->Icono       = $icono ?: null;
        $moduloModel->Orden       = $orden;

        if ($moduloModel->create()) {
            SessionHelper::setFlash('success', 'Módulo "' . $nombre . '" creado exitosamente');
        } else {
            SessionHelper::setFlash('danger', 'Error al crear el módulo');
        }

        header('Location: /vetalmacen/public/index.php?url=modulos');
        exit();
    }

    /**
     * Mostrar formulario de edición
     */
    public function editar($id) {
        PermisoHelper::requireSuperUsuario();
        AuthHelper::requireAuth();

        $moduloModel = new Modulo();
        $modulo      = $moduloModel->getById($id);

        if (!$modulo) {
            SessionHelper::setFlash('danger', 'Módulo no encontrado');
            header('Location: /vetalmacen/public/index.php?url=modulos');
            exit();
        }

        $secciones = $moduloModel->getSecciones();
        $nivel2    = $moduloModel->getModulosNivel2();

        require_once __DIR__ . '/../views/modulos/editar.php';
    }

    /**
     * Actualizar módulo existente (POST)
     */
    public function actualizar() {
        PermisoHelper::requireSuperUsuario();
        AuthHelper::requireAuth();

        if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
            header('Location: /vetalmacen/public/index.php?url=modulos');
            exit();
        }

        $id          = trim($_POST['id']          ?? '');
        $idPadre     = $_POST['id_padre']          ?? null;
        $nivel       = $_POST['nivel']             ?? '';
        $nombre      = trim($_POST['nombre']       ?? '');
        $descripcion = trim($_POST['descripcion']  ?? '');
        $ruta        = trim($_POST['ruta']         ?? '');
        $icono       = trim($_POST['icono']        ?? '');
        $orden       = $_POST['orden']             ?? 0;

        // Validaciones
        $errores = [];

        if (empty($nombre)) {
            $errores[] = 'El nombre es obligatorio';
        }

        if (empty($ruta) && $nivel != '1') {
            $errores[] = 'La ruta es obligatoria para módulos y acciones';
        }

        $moduloModel = new Modulo();

        // Verificar que el módulo existe
        if (!$moduloModel->getById($id)) {
            SessionHelper::setFlash('danger', 'Módulo no encontrado');
            header('Location: /vetalmacen/public/index.php?url=modulos');
            exit();
        }

        // Verificar ruta única excluyendo este ID
        if (!empty($ruta) && $moduloModel->rutaExiste($ruta, $id)) {
            $errores[] = 'La ruta "' . $ruta . '" ya está registrada en otro módulo';
        }

        if (!empty($errores)) {
            SessionHelper::setFlash('danger', implode('<br>', $errores));
            header('Location: /vetalmacen/public/index.php?url=modulos/editar/' . $id);
            exit();
        }

        $moduloModel->Id          = $id;
        $moduloModel->IdPadre     = $nivel == '1' ? null : $idPadre;
        $moduloModel->Nivel       = $nivel;
        $moduloModel->Nombre      = $nombre;
        $moduloModel->Descripcion = $descripcion ?: null;
        $moduloModel->Ruta        = $ruta ?: null;
        $moduloModel->Icono       = $icono ?: null;
        $moduloModel->Orden       = $orden;

        if ($moduloModel->update()) {
            SessionHelper::setFlash('success', 'Módulo "' . $nombre . '" actualizado exitosamente');
        } else {
            SessionHelper::setFlash('danger', 'Error al actualizar el módulo');
        }

        header('Location: /vetalmacen/public/index.php?url=modulos');
        exit();
    }
}
