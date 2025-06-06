-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 22-04-2025 a las 01:34:10
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.0.30

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
-- Estructura de tabla para la tabla `clientes`
--

CREATE TABLE `clientes` (
  `Id` int(11) NOT NULL,
  `UserId` int(11) NOT NULL,
  `NombreCompleto` varchar(200) NOT NULL,
  `Direccion` varchar(255) DEFAULT NULL,
  `Telefono` varchar(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `ordenes`
--

CREATE TABLE `ordenes` (
  `Id` int(11) NOT NULL,
  `ProductoId` int(11) NOT NULL,
  `UserId` int(11) NOT NULL,
  `Cantidad` int(11) NOT NULL,
  `Estado` enum('Pendiente','Confirmado','Rechazado') DEFAULT 'Pendiente',
  `FechaSolicitud` datetime DEFAULT current_timestamp(),
  `ModificadoPor` varchar(100) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `ordenes`
--

INSERT INTO `ordenes` (`Id`, `ProductoId`, `UserId`, `Cantidad`, `Estado`, `FechaSolicitud`, `ModificadoPor`) VALUES
(1, 1, 3, 1, 'Confirmado', '2025-04-16 19:37:33', 'Juan'),
(2, 1, 3, 1, 'Confirmado', '2025-04-16 20:36:34', 'Juan'),
(3, 2, 3, 2, 'Confirmado', '2025-04-16 20:37:56', 'Juan'),
(4, 1, 3, 1, 'Rechazado', '2025-04-16 21:10:30', 'Juan'),
(5, 2, 3, 2, 'Confirmado', '2025-04-16 21:10:34', 'Juan'),
(6, 1, 3, 2, 'Confirmado', '2025-04-20 20:06:44', 'Juan'),
(7, 2, 3, 4, 'Confirmado', '2025-04-20 20:15:55', 'Juan'),
(8, 2, 8, 1, 'Confirmado', '2025-04-21 14:37:30', 'Juan');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `productos`
--

CREATE TABLE `productos` (
  `id` int(11) NOT NULL,
  `codigo` varchar(50) NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `descripcion` text NOT NULL,
  `marca` varchar(100) NOT NULL,
  `categoria` varchar(100) NOT NULL,
  `precio` decimal(10,2) NOT NULL,
  `stock` int(11) NOT NULL,
  `ImagenUrl` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `productos`
--

INSERT INTO `productos` (`id`, `codigo`, `nombre`, `descripcion`, `marca`, `categoria`, `precio`, `stock`, `ImagenUrl`) VALUES
(1, '001', 'ricocat', 'comida para gatos 1kg', 'ricocat', 'comida para gatos', 10.00, 8, 'https://gopet.vtexassets.com/arquivos/ids/156416/20200359_1.jpg?v=637624771730700000'),
(2, '002', 'ricocan', 'comida para perros 1kg', 'ricocan', 'comida para perros', 20.00, 9, 'https://gopet.vtexassets.com/arquivos/ids/156399/20200311_1.jpg?v=637624771559970000');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `users`
--

CREATE TABLE `users` (
  `Id` int(11) NOT NULL,
  `Username` varchar(100) NOT NULL,
  `Password` varchar(255) NOT NULL,
  `Role` enum('Administrador','Almacenero','Cliente','Logistica') NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `users`
--

INSERT INTO `users` (`Id`, `Username`, `Password`, `Role`, `CreatedAt`) VALUES
(1, 'admin', 'admin123', 'Administrador', '2025-04-16 18:09:15'),
(2, 'almacenero', '123456', 'Almacenero', '2025-04-16 18:09:15'),
(3, 'cliente', '123456', 'Cliente', '2025-04-16 18:09:15'),
(5, 'Raul', '123456', 'Administrador', '2025-04-16 18:22:47'),
(6, 'Carlos', '123456', 'Almacenero', '2025-04-16 18:36:14'),
(7, 'Juan', '123456', 'Logistica', '2025-04-20 19:24:48'),
(8, 'katherine', '123456', 'Cliente', '2025-04-21 14:37:07');

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `clientes`
--
ALTER TABLE `clientes`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `UserId` (`UserId`);

--
-- Indices de la tabla `ordenes`
--
ALTER TABLE `ordenes`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `ProductoId` (`ProductoId`),
  ADD KEY `UserId` (`UserId`);

--
-- Indices de la tabla `productos`
--
ALTER TABLE `productos`
  ADD PRIMARY KEY (`id`);

--
-- Indices de la tabla `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`Id`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `clientes`
--
ALTER TABLE `clientes`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `ordenes`
--
ALTER TABLE `ordenes`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;

--
-- AUTO_INCREMENT de la tabla `productos`
--
ALTER TABLE `productos`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT de la tabla `users`
--
ALTER TABLE `users`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `clientes`
--
ALTER TABLE `clientes`
  ADD CONSTRAINT `clientes_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE;

--
-- Filtros para la tabla `ordenes`
--
ALTER TABLE `ordenes`
  ADD CONSTRAINT `ordenes_ibfk_1` FOREIGN KEY (`ProductoId`) REFERENCES `productos` (`id`),
  ADD CONSTRAINT `ordenes_ibfk_2` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
