# Manual del Usuario - Sistema de Gestión Veterinaria

**Versión:** 1.0  
**Fecha:** Febrero 2026  
**Destinado a:** Personal de la clínica veterinaria (recepcionistas, veterinarios, administradores)

---

## 1. Introducción

El Sistema de Gestión Veterinaria es una aplicación web diseñada para facilitar la administración diaria de una clínica veterinaria. Permite gestionar pacientes (mascotas), dueños, especies, razas y más, de forma organizada y eficiente.

### 1.1 ¿Para quién es este sistema?

| Rol | Tareas principales |
|-----|-------------------|
| **Recepcionista** | Registrar pacientes, propietarios, gestionar turnos |
| **Veterinario** | Consultar historial clínico, registrar tratamientos |
| **Administrador** | Configuración del sistema, gestión de usuarios y reportes |

---

## 2. Acceso al Sistema

1. Abrir el navegador web (se recomienda Google Chrome o Microsoft Edge)
2. Ingresar la dirección del sistema proporcionada por el administrador
3. Iniciar sesión con usuario y contraseña

---

## 3. Módulo de Especies

Las **especies** representan los tipos de animales que atiende la clínica (ej: Canino, Felino, Ave).

### 3.1 Ver listado de especies
- Navegar a **Especies** en el menú principal
- Se muestra una tabla con todas las especies activas
- Se puede filtrar por nombre usando el campo de búsqueda

### 3.2 Registrar nueva especie
1. Hacer clic en el botón **"Nueva Especie"**
2. Completar los campos:
   - **Nombre** (obligatorio): Nombre de la especie (ej: "Canino")
   - **Descripción** (opcional): Detalle adicional
3. Hacer clic en **"Guardar"**

### 3.3 Editar especie
1. En el listado, hacer clic en el ícono de **edición** (✏️)
2. Modificar los campos deseados
3. Hacer clic en **"Guardar"**

### 3.4 Eliminar especie
1. En el listado, hacer clic en el ícono de **eliminar** (🗑️)
2. Confirmar la acción en el cuadro de diálogo

> **Nota:** Eliminar una especie no borra los datos, sino que la desactiva. Los pacientes asociados no se ven afectados.

---

## 4. Módulo de Razas

Las **razas** son subdivisiones de cada especie (ej: "Golden Retriever" dentro de "Canino").

### 4.1 Ver listado de razas
- Navegar a **Razas** en el menú principal
- Se puede filtrar por especie usando el selector desplegable

### 4.2 Registrar nueva raza
1. Hacer clic en **"Nueva Raza"**
2. Completar los campos:
   - **Nombre** (obligatorio): Nombre de la raza
   - **Especie** (obligatorio): Seleccionar la especie correspondiente
   - **Descripción** (opcional)
3. Hacer clic en **"Guardar"**

### 4.3 Editar y eliminar razas
- Mismo procedimiento que para Especies (ver secciones 3.3 y 3.4)

---

## 5. Módulo de Propietarios (Dueños)

Los **propietarios** son las personas responsables de las mascotas.

### 5.1 Ver listado de propietarios
- Navegar a **Propietarios** en el menú principal
- Se puede buscar por nombre o DNI

### 5.2 Registrar nuevo propietario
1. Hacer clic en **"Nuevo Propietario"**
2. Completar los campos:
   - **Nombre** (obligatorio)
   - **Apellido** (obligatorio)
   - **DNI** (obligatorio, único por propietario)
   - **Teléfono** (obligatorio)
   - **Email** (opcional, debe ser un email válido)
   - **Dirección** (opcional)
3. Hacer clic en **"Guardar"**

> **Importante:** No se pueden registrar dos propietarios con el mismo DNI.

### 5.3 Buscar propietario por DNI
1. Ir a **Propietarios**
2. Usar el campo de búsqueda e ingresar el DNI completo

### 5.4 Editar propietario
1. Hacer clic en el ícono de **edición**
2. Se pueden modificar todos los campos excepto el DNI
3. Hacer clic en **"Guardar"**

---

## 6. Módulo de Pacientes (Mascotas)

Los **pacientes** son las mascotas que se atienden en la clínica.

### 6.1 Ver listado de pacientes
- Navegar a **Pacientes** en el menú principal
- Se puede filtrar por:
  - Nombre de la mascota
  - Propietario
  - Especie

### 6.2 Registrar nuevo paciente
1. Hacer clic en **"Nuevo Paciente"**
2. Completar los campos:
   - **Nombre** (obligatorio): Nombre de la mascota
   - **Especie** (obligatorio): Seleccionar del listado
   - **Raza** (opcional): Se filtra automáticamente según la especie seleccionada
   - **Sexo** (obligatorio): M (Macho) o H (Hembra)
   - **Propietario** (obligatorio): Buscar y seleccionar el dueño
   - **Fecha de Nacimiento** (opcional): Para calcular la edad automáticamente
   - **Foto** (opcional): URL de la foto de la mascota
   - **Observaciones** (opcional): Notas adicionales
3. Hacer clic en **"Guardar"**

### 6.3 Ver detalle del paciente
- Hacer clic sobre el nombre del paciente en el listado
- Se muestra:
  - Datos del paciente
  - Edad calculada automáticamente
  - Datos del propietario
  - Especie y raza

### 6.4 Cambiar propietario de un paciente
1. En el detalle del paciente, hacer clic en **"Cambiar Propietario"**
2. Buscar y seleccionar el nuevo propietario
3. Confirmar el cambio

---

## 7. Glosario

| Término | Significado |
|---------|-------------|
| **Especie** | Tipo de animal (Canino, Felino, Ave, etc.) |
| **Raza** | Subdivisión de la especie (Golden Retriever, Siamés, etc.) |
| **Propietario** | Persona responsable de la mascota |
| **Paciente** | Mascota registrada en el sistema |
| **DNI** | Documento Nacional de Identidad del propietario |
| **Soft Delete** | Al eliminar un registro, este se desactiva pero no se borra de la base de datos |

---

## 8. Preguntas Frecuentes (FAQ)

**¿Puedo recuperar un registro eliminado?**  
Sí. Los registros eliminados se desactivan, no se borran. Un administrador puede reactivarlos.

**¿Qué pasa si registro un propietario con un DNI que ya existe?**  
El sistema mostrará un error indicando que el DNI ya está registrado.

**¿Puedo cambiar la especie de un paciente?**  
Sí, pero al cambiar la especie se actualizará también la raza.

**¿Es obligatorio registrar la fecha de nacimiento del paciente?**  
No, es opcional. Si se ingresa, el sistema calculará la edad automáticamente.
