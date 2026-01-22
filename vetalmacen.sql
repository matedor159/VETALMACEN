-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 21-01-2026 a las 23:46:32
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `vetalmacen`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `categoria`
--

CREATE TABLE `categoria` (
  `Id` int(11) NOT NULL,
  `Nombre` varchar(150) NOT NULL,
  `Descripcion` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `categoria`
--

INSERT INTO `categoria` (`Id`, `Nombre`, `Descripcion`) VALUES
(1, 'Perro', 'Ítems para perros'),
(2, 'Gato', 'Ítems para gatos');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `detalleordenentrada`
--

CREATE TABLE `detalleordenentrada` (
  `Id` int(11) NOT NULL,
  `OrdenEntradaId` int(11) NOT NULL,
  `ProductoId` int(11) NOT NULL,
  `Cantidad` int(11) NOT NULL,
  `PrecioUnitario` decimal(12,2) NOT NULL,
  `SubTotal` decimal(12,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Disparadores `detalleordenentrada`
--
DELIMITER $$
CREATE TRIGGER `trg_detalleOrdenEntrada_after_delete` AFTER DELETE ON `detalleordenentrada` FOR EACH ROW BEGIN
  UPDATE Producto
    SET Stock = Stock - OLD.Cantidad
    WHERE Id = OLD.ProductoId;
END
$$
DELIMITER ;
DELIMITER $$
CREATE TRIGGER `trg_detalleOrdenEntrada_after_insert` AFTER INSERT ON `detalleordenentrada` FOR EACH ROW BEGIN
  UPDATE Producto
    SET Stock = Stock + NEW.Cantidad
    WHERE Id = NEW.ProductoId;
END
$$
DELIMITER ;
DELIMITER $$
CREATE TRIGGER `trg_detalleOrdenEntrada_after_update` AFTER UPDATE ON `detalleordenentrada` FOR EACH ROW BEGIN
  IF NEW.ProductoId = OLD.ProductoId THEN
    UPDATE Producto
      SET Stock = Stock + (NEW.Cantidad - OLD.Cantidad)
      WHERE Id = NEW.ProductoId;
  ELSE
    UPDATE Producto
      SET Stock = Stock - OLD.Cantidad
      WHERE Id = OLD.ProductoId;
    UPDATE Producto
      SET Stock = Stock + NEW.Cantidad
      WHERE Id = NEW.ProductoId;
  END IF;
END
$$
DELIMITER ;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `detalleordensalida`
--

CREATE TABLE `detalleordensalida` (
  `Id` int(11) NOT NULL,
  `OrdenSalidaId` int(11) NOT NULL,
  `ProductoId` int(11) NOT NULL,
  `Cantidad` int(11) NOT NULL,
  `PrecioUnitario` decimal(12,2) NOT NULL,
  `SubTotal` decimal(12,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Disparadores `detalleordensalida`
--
DELIMITER $$
CREATE TRIGGER `trg_detalleOrdenSalida_after_delete` AFTER DELETE ON `detalleordensalida` FOR EACH ROW BEGIN
  UPDATE Producto
    SET Stock = Stock + OLD.Cantidad
    WHERE Id = OLD.ProductoId;
END
$$
DELIMITER ;
DELIMITER $$
CREATE TRIGGER `trg_detalleOrdenSalida_after_insert` AFTER INSERT ON `detalleordensalida` FOR EACH ROW BEGIN
  UPDATE Producto
    SET Stock = Stock - NEW.Cantidad
    WHERE Id = NEW.ProductoId;
END
$$
DELIMITER ;
DELIMITER $$
CREATE TRIGGER `trg_detalleOrdenSalida_after_update` AFTER UPDATE ON `detalleordensalida` FOR EACH ROW BEGIN
  IF NEW.ProductoId = OLD.ProductoId THEN
    UPDATE Producto
      SET Stock = Stock - (NEW.Cantidad - OLD.Cantidad)
      WHERE Id = NEW.ProductoId;
  ELSE
    UPDATE Producto
      SET Stock = Stock + OLD.Cantidad
      WHERE Id = OLD.ProductoId;
    UPDATE Producto
      SET Stock = Stock - NEW.Cantidad
      WHERE Id = NEW.ProductoId;
  END IF;
END
$$
DELIMITER ;
DELIMITER $$
CREATE TRIGGER `trg_detalleOrdenSalida_before_insert` BEFORE INSERT ON `detalleordensalida` FOR EACH ROW BEGIN
  DECLARE v_stock INT DEFAULT 0;
  SELECT Stock INTO v_stock FROM Producto WHERE Id = NEW.ProductoId FOR UPDATE;
  IF v_stock < NEW.Cantidad THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Stock insuficiente para la salida';
  END IF;
END
$$
DELIMITER ;
DELIMITER $$
CREATE TRIGGER `trg_detalleOrdenSalida_before_update` BEFORE UPDATE ON `detalleordensalida` FOR EACH ROW BEGIN
  DECLARE v_stock INT DEFAULT 0;
  DECLARE v_diff INT DEFAULT 0;
  SET v_diff = NEW.Cantidad - OLD.Cantidad;
  IF v_diff > 0 THEN
    SELECT Stock INTO v_stock FROM Producto WHERE Id = NEW.ProductoId FOR UPDATE;
    IF v_stock < v_diff THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Stock insuficiente para actualizar la salida';
    END IF;
  END IF;
END
$$
DELIMITER ;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `ordenentrada`
--

CREATE TABLE `ordenentrada` (
  `Id` int(11) NOT NULL,
  `ProveedorId` int(11) NOT NULL,
  `UsuarioId` int(11) NOT NULL,
  `Fecha` datetime NOT NULL DEFAULT current_timestamp(),
  `Estado` enum('Pendiente','Recibido','Cancelado') DEFAULT 'Pendiente',
  `Total` decimal(12,2) DEFAULT 0.00,
  `Observacion` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `ordensalida`
--

CREATE TABLE `ordensalida` (
  `Id` int(11) NOT NULL,
  `UsuarioId` int(11) NOT NULL,
  `SucursalId` int(11) DEFAULT NULL,
  `Fecha` datetime NOT NULL DEFAULT current_timestamp(),
  `TipoSalida` enum('Venta','Transferencia','SalidaInterna') DEFAULT 'Venta',
  `Estado` enum('Pendiente','Procesado','Cancelado') DEFAULT 'Pendiente',
  `Total` decimal(12,2) DEFAULT 0.00,
  `Observacion` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `producto`
--

CREATE TABLE `producto` (
  `Id` int(11) NOT NULL,
  `Codigo` varchar(50) NOT NULL,
  `Nombre` varchar(100) NOT NULL,
  `Descripcion` text NOT NULL,
  `Marca` varchar(100) NOT NULL,
  `SubCategoriaId` int(11) NOT NULL,
  `Precio` decimal(10,2) NOT NULL,
  `Stock` int(11) NOT NULL DEFAULT 0,
  `ImagenUrl` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `producto`
--

INSERT INTO `producto` (`Id`, `Codigo`, `Nombre`, `Descripcion`, `Marca`, `SubCategoriaId`, `Precio`, `Stock`, `ImagenUrl`) VALUES
(1, '001', 'Ricocat', 'Comida para gatos 1kg', 'Ricocat', 1, 10.00, 8, 'https://gopet.vtexassets.com/arquivos/ids/156416/20200359_1.jpg'),
(2, '002', 'Ricocan', 'Comida para perros 1kg', 'Ricocan', 2, 20.00, 9, 'https://gopet.vtexassets.com/arquivos/ids/156399/20200311_1.jpg');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `proveedor`
--

CREATE TABLE `proveedor` (
  `Id` int(11) NOT NULL,
  `RazonSocial` varchar(200) NOT NULL,
  `RUC` varchar(20) DEFAULT NULL,
  `NombreContacto` varchar(150) DEFAULT NULL,
  `Direccion` varchar(255) DEFAULT NULL,
  `Telefono` varchar(50) DEFAULT NULL,
  `Email` varchar(150) DEFAULT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `proveedor`
--

INSERT INTO `proveedor` (`Id`, `RazonSocial`, `RUC`, `NombreContacto`, `Direccion`, `Telefono`, `Email`, `CreatedAt`) VALUES
(1, 'Proveedor Demo S.A.', '20123456789', 'Carlos Perez', 'Av. Principal 123', '+51 987654321', 'contacto@proveedordemo.com', '2026-01-21 00:32:50');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `rol`
--

CREATE TABLE `rol` (
  `Id` int(11) NOT NULL,
  `Nombre` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `rol`
--

INSERT INTO `rol` (`Id`, `Nombre`) VALUES
(1, 'Administrador'),
(2, 'Almacenero'),
(3, 'Logistica'),
(4, 'Sucursal');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `subcategoria`
--

CREATE TABLE `subcategoria` (
  `Id` int(11) NOT NULL,
  `CategoriaId` int(11) NOT NULL,
  `Nombre` varchar(150) NOT NULL,
  `Descripcion` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `subcategoria`
--

INSERT INTO `subcategoria` (`Id`, `CategoriaId`, `Nombre`, `Descripcion`) VALUES
(1, 2, 'Comida Para Gatos', 'Alimentos para gatos'),
(2, 1, 'Comida Para Perros', 'Alimentos para perros');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `sucursal`
--

CREATE TABLE `sucursal` (
  `Id` int(11) NOT NULL,
  `Sede` varchar(150) NOT NULL,
  `Direccion` varchar(255) NOT NULL,
  `Telefono` varchar(50) DEFAULT NULL,
  `Email` varchar(150) DEFAULT NULL,
  `HorarioEntrega` varchar(100) DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT 1,
  `CreatedAt` datetime NOT NULL DEFAULT current_timestamp(),
  `UpdatedAt` datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `sucursal`
--

INSERT INTO `sucursal` (`Id`, `Sede`, `Direccion`, `Telefono`, `Email`, `HorarioEntrega`, `Activo`, `CreatedAt`, `UpdatedAt`) VALUES
(1, 'Los Olivos', 'Av. Example 123, Los Olivos', '+51 987000111', 'losolivos@vetalmacen.com', 'L-V 08:00-18:00', 1, '2026-01-21 00:32:50', '2026-01-21 00:32:50');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuario`
--

CREATE TABLE `usuario` (
  `Id` int(11) NOT NULL,
  `Username` varchar(100) NOT NULL,
  `Password` text NOT NULL,
  `RolId` int(11) NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `usuario`
--

INSERT INTO `usuario` (`Id`, `Username`, `Password`, `RolId`, `CreatedAt`) VALUES
(1, 'admin', 'admin123', 1, '2025-04-16 18:09:15'),
(2, 'almacenero', '123456', 2, '2025-04-16 18:09:15'),
(3, 'sede_user', '123456', 4, '2025-04-16 18:09:15'),
(5, 'Raul', '123456', 1, '2025-04-16 18:22:47'),
(6, 'Carlos', '123456', 2, '2025-04-16 18:36:14'),
(7, 'Juan', '123456', 3, '2025-04-20 19:24:48'),
(8, 'katherine', '123456', 4, '2025-04-21 14:37:07');

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `categoria`
--
ALTER TABLE `categoria`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `uq_categoria_nombre` (`Nombre`);

--
-- Indices de la tabla `detalleordenentrada`
--
ALTER TABLE `detalleordenentrada`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `uq_detalleOrdenEntrada_line` (`OrdenEntradaId`,`ProductoId`),
  ADD KEY `idx_detalleOrdenEntrada_orden` (`OrdenEntradaId`),
  ADD KEY `idx_detalleOrdenEntrada_producto` (`ProductoId`);

--
-- Indices de la tabla `detalleordensalida`
--
ALTER TABLE `detalleordensalida`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `uq_detalleOrdenSalida_line` (`OrdenSalidaId`,`ProductoId`),
  ADD KEY `idx_detalleOrdenSalida_orden` (`OrdenSalidaId`),
  ADD KEY `idx_detalleOrdenSalida_producto` (`ProductoId`);

--
-- Indices de la tabla `ordenentrada`
--
ALTER TABLE `ordenentrada`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `idx_ordenentrada_proveedor` (`ProveedorId`),
  ADD KEY `idx_ordenentrada_usuario` (`UsuarioId`);

--
-- Indices de la tabla `ordensalida`
--
ALTER TABLE `ordensalida`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `idx_ordensalida_usuario` (`UsuarioId`),
  ADD KEY `idx_ordensalida_sucursal` (`SucursalId`);

--
-- Indices de la tabla `producto`
--
ALTER TABLE `producto`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `idx_producto_subcategoria` (`SubCategoriaId`);

--
-- Indices de la tabla `proveedor`
--
ALTER TABLE `proveedor`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `uq_proveedor_razonsocial` (`RazonSocial`),
  ADD UNIQUE KEY `uq_proveedor_ruc` (`RUC`);

--
-- Indices de la tabla `rol`
--
ALTER TABLE `rol`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `uq_rol_nombre` (`Nombre`);

--
-- Indices de la tabla `subcategoria`
--
ALTER TABLE `subcategoria`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `uq_subcategoria_categoria_nombre` (`CategoriaId`,`Nombre`),
  ADD KEY `idx_subcategoria_categoria` (`CategoriaId`);

--
-- Indices de la tabla `sucursal`
--
ALTER TABLE `sucursal`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `uq_sucursal_sede_direccion` (`Sede`,`Direccion`);

--
-- Indices de la tabla `usuario`
--
ALTER TABLE `usuario`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `uq_usuario_username` (`Username`),
  ADD KEY `idx_usuario_rol` (`RolId`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `categoria`
--
ALTER TABLE `categoria`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT de la tabla `detalleordenentrada`
--
ALTER TABLE `detalleordenentrada`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `detalleordensalida`
--
ALTER TABLE `detalleordensalida`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `ordenentrada`
--
ALTER TABLE `ordenentrada`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `ordensalida`
--
ALTER TABLE `ordensalida`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `producto`
--
ALTER TABLE `producto`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT de la tabla `proveedor`
--
ALTER TABLE `proveedor`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT de la tabla `rol`
--
ALTER TABLE `rol`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT de la tabla `subcategoria`
--
ALTER TABLE `subcategoria`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT de la tabla `sucursal`
--
ALTER TABLE `sucursal`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT de la tabla `usuario`
--
ALTER TABLE `usuario`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `detalleordenentrada`
--
ALTER TABLE `detalleordenentrada`
  ADD CONSTRAINT `fk_detalleOrdenEntrada_orden` FOREIGN KEY (`OrdenEntradaId`) REFERENCES `ordenentrada` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_detalleOrdenEntrada_producto` FOREIGN KEY (`ProductoId`) REFERENCES `producto` (`Id`) ON UPDATE CASCADE;

--
-- Filtros para la tabla `detalleordensalida`
--
ALTER TABLE `detalleordensalida`
  ADD CONSTRAINT `fk_detalleOrdenSalida_orden` FOREIGN KEY (`OrdenSalidaId`) REFERENCES `ordensalida` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_detalleOrdenSalida_producto` FOREIGN KEY (`ProductoId`) REFERENCES `producto` (`Id`) ON UPDATE CASCADE;

--
-- Filtros para la tabla `ordenentrada`
--
ALTER TABLE `ordenentrada`
  ADD CONSTRAINT `fk_ordenentrada_proveedor` FOREIGN KEY (`ProveedorId`) REFERENCES `proveedor` (`Id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_ordenentrada_usuario` FOREIGN KEY (`UsuarioId`) REFERENCES `usuario` (`Id`) ON UPDATE CASCADE;

--
-- Filtros para la tabla `ordensalida`
--
ALTER TABLE `ordensalida`
  ADD CONSTRAINT `fk_ordensalida_sucursal` FOREIGN KEY (`SucursalId`) REFERENCES `sucursal` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_ordensalida_usuario` FOREIGN KEY (`UsuarioId`) REFERENCES `usuario` (`Id`) ON UPDATE CASCADE;

--
-- Filtros para la tabla `producto`
--
ALTER TABLE `producto`
  ADD CONSTRAINT `fk_producto_subcategoria` FOREIGN KEY (`SubCategoriaId`) REFERENCES `subcategoria` (`Id`) ON UPDATE CASCADE;

--
-- Filtros para la tabla `subcategoria`
--
ALTER TABLE `subcategoria`
  ADD CONSTRAINT `fk_subcategoria_categoria` FOREIGN KEY (`CategoriaId`) REFERENCES `categoria` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Filtros para la tabla `usuario`
--
ALTER TABLE `usuario`
  ADD CONSTRAINT `fk_usuario_rol` FOREIGN KEY (`RolId`) REFERENCES `rol` (`Id`) ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
