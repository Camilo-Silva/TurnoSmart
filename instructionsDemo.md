# 🏥 TurnoSmart - Instrucciones de Demo

## 🌐 URL de la Demo
**Producción**: https://turnosmart-production.up.railway.app

## 🔐 Usuarios de Prueba

### 👨‍💼 Administrador
- **Email**: `admin@turno-smart.com.ar`
- **Password**: `Admin123!`
- **Rol**: Administrador completo del sistema

### 👤 Paciente de Prueba
- **Email**: `paciente15@turno-smart.com.ar`
- **Password**: `cualquiera1`
- **Rol**: Paciente registrado

### ⚕️ Médico de Prueba
- **Email**: `medico1@turno-smart.com.ar`
- **Password**: `NuevoMedic0!`
- **Rol**: Médico con especialidad

### 👩‍💼 Recepcionista de Prueba
- **Email**: `recepcion1@turno-smart.com.ar`
- **Password**: `Recep123!`
- **Rol**: Recepcionista del centro médico

---

## 🚀 Flujo de la Aplicación

### 1. 👨‍💼 Como Administrador

**Iniciar Sesión:**
1. Ir a la página principal
2. Hacer clic en "Iniciar Sesión"
3. Ingresar credenciales de administrador
4. Acceder al panel completo

**Funcionalidades del Admin:**
- ✅ **Ver Dashboard**: Resumen general del sistema
- ✅ **Gestión de Médicos**: Ver, crear, editar y eliminar médicos
- ✅ **Gestión de Pacientes**: Administrar base de pacientes
- ✅ **Especialidades**: Gestionar especialidades médicas
- ✅ **Turnos**: Ver y gestionar todos los turnos del sistema
- ✅ **Historiales Médicos**: Acceso completo a historiales

**Navegación Admin:**
```
Navbar → Médicos → Ver lista de médicos registrados
Navbar → Pacientes → Ver pacientes dados de alta
Navbar → Especialidades → Gestionar especialidades
Navbar → Turnos → Ver agenda completa
```

---

### 2. 👤 Como Paciente

#### Opción A: Usar Paciente Existente
1. **Iniciar Sesión** con `paciente15@turno-smart.com.ar`
2. **Ver Perfil**: Datos personales completos
3. **Reservar Turno**: Navegar por especialidades y médicos

#### Opción B: Registrar Nuevo Paciente
1. **Registro**: Hacer clic en "Registrarse"
2. **Completar Formulario**:
   - Nombre y apellido
   - DNI único
   - Email
   - Fecha de nacimiento
   - Contraseña
   - ✅ Aceptar términos
3. **Confirmación**: Sistema crea usuario automáticamente

**Funcionalidades del Paciente:**
- ✅ **Mi Perfil**: Ver y editar datos personales
- ✅ **Reservar Turnos**: Buscar por especialidad o médico
- ✅ **Mis Turnos**: Ver turnos reservados
- ✅ **Cambiar Contraseña**: Gestión de cuenta

**Flujo de Reserva de Turno:**
```
1. Especialidades → Seleccionar especialidad
2. Médicos → Elegir profesional
3. Horarios → Seleccionar fecha y hora disponible
4. Confirmar → Turno reservado
```

---

### 3. ⚕️ Como Médico

**Iniciar Sesión:**
1. Usar credenciales: `medico1@turno-smart.com.ar`
2. Acceder al panel médico

**Funcionalidades del Médico:**
- ✅ **Mi Agenda**: Ver turnos del día/semana
- ✅ **Historiales Médicos**: Crear y gestionar historiales
- ✅ **Pacientes**: Ver lista de pacientes asignados
- ✅ **Turnos**: Confirmar y gestionar citas

**Gestión de Historiales:**
```
1. Turnos → Seleccionar paciente
2. Historial Médico → Crear nuevo
3. Completar:
   - Síntomas
   - Diagnóstico
   - Tratamiento
   - Prescripciones
   - Notas adicionales
4. Guardar historial
```

---

### 4. 👩‍💼 Como Recepcionista

**Iniciar Sesión:**
1. Usar credenciales: `recepcion1@turno-smart.com.ar`
2. Acceder al panel de recepción

**Funcionalidades de la Recepcionista:**
- ✅ **Gestión de Pacientes**: Registrar, editar y administrar pacientes
- ✅ **Confirmar Agenda Médica**: Verificar y confirmar turnos de médicos
- ✅ **Turnos**: Ver y gestionar la agenda general
- ✅ **Atención al Paciente**: Primera línea de contacto

**Flujo de Trabajo Recepcionista:**
```
1. Gestión de Pacientes:
   - Registrar nuevos pacientes
   - Actualizar datos existentes
   - Verificar información de contacto

2. Confirmación de Agenda:
   - Revisar turnos programados
   - Confirmar disponibilidad médica
   - Coordinar horarios y especialidades

3. Atención y Seguimiento:
   - Recibir consultas de pacientes
   - Gestionar cambios de turno
   - Coordinar con médicos
```

---

## 🎯 Flujos de Prueba Recomendados

### Flujo Completo de Turno

1. **Admin**: Verificar médicos y especialidades disponibles
2. **Paciente**: Registrarse o iniciar sesión
3. **Paciente**: Reservar turno navegando:
   ```
   Especialidades → Cardiología
   Médicos → Dr. Ejemplo
   Horarios → Seleccionar fecha/hora
   Confirmar → ✅ Turno reservado
   ```
4. **Recepcionista**: Confirmar agenda médica y verificar disponibilidad
5. **Admin**: Verificar turno en el sistema
6. **Médico**: Ver turno en agenda
7. **Médico**: Crear historial médico post-consulta

### Flujo de Gestión Recepcionista

1. **Gestión de Pacientes**:
   - Registrar nuevos pacientes
   - Actualizar datos de contacto
   - Verificar información personal
2. **Confirmación de Agenda**:
   - Revisar turnos del día
   - Confirmar disponibilidad médica
   - Coordinar horarios y especialidades
3. **Atención al Cliente**:
   - Gestionar consultas telefónicas
   - Coordinar cambios de turno
   - Facilitar comunicación médico-paciente

### Flujo de Gestión Admin

1. **Ver Estadísticas**: Dashboard principal
2. **Gestionar Médicos**: 
   - Ver lista completa
   - Crear nuevo médico
   - Asignar especialidad
3. **Gestionar Pacientes**:
   - Ver pacientes registrados
   - Editar información
   - Ver historiales

---

## 📱 Navegación del Sistema

### Navbar Principal
- **🏠 Inicio**: Página principal
- **⚕️ Médicos**: Listado de profesionales
- **🏥 Especialidades**: Categorías médicas
- **📅 Turnos**: Sistema de citas (según rol)
- **👤 Perfil**: Datos del usuario logueado

### Navegación por Rol

**Admin ve:**
- Médicos, Pacientes, Especialidades, Turnos, Historiales

**Paciente ve:**
- Especialidades, Médicos, Mis Turnos, Mi Perfil

**Médico ve:**
- Mi Agenda, Mis Pacientes, Historiales, Mi Perfil

**Recepcionista ve:**
- Pacientes, Turnos, Agenda Médica, Mi Perfil

---

## 🔍 Características a Probar

### ✅ Funcionalidades Principales
- **Autenticación**: Login/logout fluido
- **Responsive**: Funciona en móvil y desktop
- **Modales**: Formularios dinámicos
- **Validaciones**: Cliente y servidor
- **CRUD Completo**: En todas las entidades
- **Relaciones**: Usuario↔Paciente por DNI
- **Fechas UTC**: Compatible con PostgreSQL

### ✅ Detalles Técnicos
- **Base de Datos**: PostgreSQL en Railway
- **SSL**: Conexión segura
- **Deploy**: Automático desde GitHub
- **Logs**: Tiempo real en Railway

---

## 🎉 ¡Explora TurnoSmart!

La aplicación está completamente funcional. Puedes probar todos los roles y funcionalidades usando las credenciales proporcionadas. 

**¿Problemas?** Contacta al desarrollador a través del repositorio GitHub.
