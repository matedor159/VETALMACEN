<?php
require_once __DIR__ . '/Database.php';

class Modulo {
    private $conn;
    private $table = 'modulo';

    public $Id;
    public $IdPadre;
    public $Nivel;
    public $Nombre;
    public $Descripcion;
    public $Ruta;
    public $Icono;
    public $Orden;
    public $Activo;

    public function __construct() {
        $database = new Database();
        $this->conn = $database->getConnection();
    }

    /**
     * Obtener todos los módulos activos
     */
    public function getAll() {
        $query = "SELECT * FROM " . $this->table . " 
                  WHERE Activo = 1 
                  ORDER BY Orden ASC, Nombre ASC";
        
        $stmt = $this->conn->prepare($query);
        $stmt->execute();
        return $stmt->fetchAll();
    }

    /**
     * Obtener TODOS los módulos (activos e inactivos) para el CRUD
     */
    public function getAllIncludingInactivos() {
        $query = "SELECT * FROM " . $this->table . " 
                  ORDER BY Nivel ASC, Orden ASC, Nombre ASC";
        
        $stmt = $this->conn->prepare($query);
        $stmt->execute();
        return $stmt->fetchAll();
    }

    /**
     * Obtener secciones (Nivel 1) activas
     */
    public function getSecciones() {
        $query = "SELECT * FROM " . $this->table . " 
                  WHERE Nivel = 1 AND Activo = 1 
                  ORDER BY Orden ASC";
        
        $stmt = $this->conn->prepare($query);
        $stmt->execute();
        return $stmt->fetchAll();
    }

    /**
     * Obtener módulos de nivel 2 (para selector de padre al crear Acción)
     */
    public function getModulosNivel2() {
        $query = "SELECT * FROM " . $this->table . " 
                  WHERE Nivel = 2 AND Activo = 1 
                  ORDER BY Orden ASC";
        
        $stmt = $this->conn->prepare($query);
        $stmt->execute();
        return $stmt->fetchAll();
    }

    /**
     * Obtener módulos hijos de un padre (activos)
     */
    public function getHijosByPadre($padreId) {
        $query = "SELECT * FROM " . $this->table . " 
                  WHERE IdPadre = :padre_id AND Activo = 1 
                  ORDER BY Orden ASC";
        
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(':padre_id', $padreId);
        $stmt->execute();
        return $stmt->fetchAll();
    }

    /**
     * Obtener TODOS los hijos (activos e inactivos) - para CRUD
     */
    public function getTodosHijosByPadre($padreId) {
        $query = "SELECT * FROM " . $this->table . " 
                  WHERE IdPadre = :padre_id
                  ORDER BY Orden ASC";
        
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(':padre_id', $padreId);
        $stmt->execute();
        return $stmt->fetchAll();
    }

    /**
     * Obtener módulo por ID
     */
    public function getById($id) {
        $query = "SELECT * FROM " . $this->table . " WHERE Id = :id";
        
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(':id', $id);
        $stmt->execute();
        return $stmt->fetch();
    }

    /**
     * Obtener módulo por ruta
     */
    public function getByRuta($ruta) {
        $query = "SELECT * FROM " . $this->table . " 
                  WHERE Ruta = :ruta
                  LIMIT 1";
        
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(':ruta', $ruta);
        $stmt->execute();
        return $stmt->fetch();
    }

    /**
     * Verificar si la ruta ya existe (excluyendo un ID)
     * Útil al editar para no comparar consigo mismo
     */
    public function rutaExiste($ruta, $excludeId = null) {
        $query = "SELECT COUNT(*) as total FROM " . $this->table . " 
                  WHERE Ruta = :ruta";
        
        if ($excludeId) {
            $query .= " AND Id != :exclude_id";
        }
        
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(':ruta', $ruta);
        
        if ($excludeId) {
            $stmt->bindParam(':exclude_id', $excludeId);
        }
        
        $stmt->execute();
        $result = $stmt->fetch();
        return $result['total'] > 0;
    }

    /**
     * Verificar si el módulo tiene hijos
     * Útil antes de eliminar
     */
    public function tieneHijos($id) {
        $query = "SELECT COUNT(*) as total FROM " . $this->table . " 
                  WHERE IdPadre = :id";
        
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(':id', $id);
        $stmt->execute();
        $result = $stmt->fetch();
        return $result['total'] > 0;
    }

    /**
     * Verificar si el módulo tiene permisos asignados
     * Útil antes de eliminar
     */
    public function tienePermisos($id) {
        $query = "SELECT COUNT(*) as total FROM permiso 
                  WHERE ModuloId = :id";
        
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(':id', $id);
        $stmt->execute();
        $result = $stmt->fetch();
        return $result['total'] > 0;
    }

    /**
     * Obtener árbol jerárquico completo (solo activos)
     * Usado por navbar y PermisoHelper
     */
    public function getArbolCompleto() {
        $secciones = $this->getSecciones();
        $arbol = [];
        
        foreach ($secciones as $seccion) {
            $seccion['modulos'] = $this->getHijosByPadre($seccion['Id']);
            
            foreach ($seccion['modulos'] as &$modulo) {
                $modulo['acciones'] = $this->getHijosByPadre($modulo['Id']);
            }
            
            $arbol[] = $seccion;
        }
        
        return $arbol;
    }

    /**
     * Obtener árbol jerárquico COMPLETO incluyendo inactivos
     * Usado por el CRUD de módulos
     */
    public function getArbolCompletoConInactivos() {
        $query = "SELECT * FROM " . $this->table . " 
                  WHERE Nivel = 1
                  ORDER BY Orden ASC";
        $stmt = $this->conn->prepare($query);
        $stmt->execute();
        $secciones = $stmt->fetchAll();

        $arbol = [];

        foreach ($secciones as $seccion) {
            $seccion['modulos'] = $this->getTodosHijosByPadre($seccion['Id']);

            foreach ($seccion['modulos'] as &$modulo) {
                $modulo['acciones'] = $this->getTodosHijosByPadre($modulo['Id']);
            }

            $arbol[] = $seccion;
        }

        return $arbol;
    }

    /**
     * Crear nuevo módulo
     */
    public function create() {
        $query = "INSERT INTO " . $this->table . " 
                  (Id, IdPadre, Nivel, Nombre, Descripcion, Ruta, Icono, Orden, Activo)
                  VALUES (:id, :id_padre, :nivel, :nombre, :descripcion, :ruta, :icono, :orden, 1)";
        
        $stmt = $this->conn->prepare($query);
        
        $stmt->bindParam(':id',          $this->Id);
        $stmt->bindParam(':id_padre',    $this->IdPadre);
        $stmt->bindParam(':nivel',       $this->Nivel);
        $stmt->bindParam(':nombre',      $this->Nombre);
        $stmt->bindParam(':descripcion', $this->Descripcion);
        $stmt->bindParam(':ruta',        $this->Ruta);
        $stmt->bindParam(':icono',       $this->Icono);
        $stmt->bindParam(':orden',       $this->Orden);
        
        return $stmt->execute();
    }

    /**
     * Actualizar módulo existente
     */
    public function update() {
        $query = "UPDATE " . $this->table . " 
                  SET IdPadre     = :id_padre,
                      Nivel       = :nivel,
                      Nombre      = :nombre,
                      Descripcion = :descripcion,
                      Ruta        = :ruta,
                      Icono       = :icono,
                      Orden       = :orden
                  WHERE Id = :id";
        
        $stmt = $this->conn->prepare($query);
        
        $stmt->bindParam(':id',          $this->Id);
        $stmt->bindParam(':id_padre',    $this->IdPadre);
        $stmt->bindParam(':nivel',       $this->Nivel);
        $stmt->bindParam(':nombre',      $this->Nombre);
        $stmt->bindParam(':descripcion', $this->Descripcion);
        $stmt->bindParam(':ruta',        $this->Ruta);
        $stmt->bindParam(':icono',       $this->Icono);
        $stmt->bindParam(':orden',       $this->Orden);
        
        return $stmt->execute();
    }

    /**
     * Eliminar módulo
     * Solo si no tiene hijos ni permisos asignados
     */
    public function delete() {
        $query = "DELETE FROM " . $this->table . " WHERE Id = :id";
        
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(':id', $this->Id);
        
        return $stmt->execute();
    }

    /**
     * Activar o desactivar módulo
     */
    public function toggleActivo() {
        $query = "UPDATE " . $this->table . " 
                  SET Activo = IF(Activo = 1, 0, 1)
                  WHERE Id = :id";
        
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(':id', $this->Id);
        
        return $stmt->execute();
    }

    /**
     * Obtener siguiente ID disponible para un prefijo dado
     * Útil para sugerir el ID al crear módulos manualmente
     * 
     * @param int $prefijo - Ej: 5005 para acciones bajo 5005xx
     */
    public function getSiguienteId($prefijo) {
        $query = "SELECT MAX(Id) as max_id FROM " . $this->table . " 
                  WHERE Id >= :min AND Id < :max";
        
        $min = $prefijo * 100;
        $max = ($prefijo + 1) * 100;
        
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(':min', $min);
        $stmt->bindParam(':max', $max);
        $stmt->execute();
        
        $result = $stmt->fetch();
        
        if ($result['max_id']) {
            return $result['max_id'] + 1;
        }
        
        return $min + 1;
    }
}
