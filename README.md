Instalación


Está codificado con Laravel así que hay que seguir unos pasos para instalar el proyecto antes de empezar.

Paso 1 - Tener instalado/instalar el Xampp
Paso 2 - En la carpeta htdocs del xampp crear una carpeta para los proyectos si no la tienen y alli clonar el proyecto
Paso 3  - Crear una base de datos en el MySql para poderla usar
Paso 4  - Copiar y pegar el archivo env.example, renombrarlo a env y Configurar el archivo para la maquina
Paso 5  - Instalar Composer https://getcomposer.org/download/ , reiniciar el dispositivo
Paso 6  - Abrir el VisualCode y cargar la carpeta del proyecto
Paso 7  - consola: composer global require laravel/installer
Paso 8  - consola: php artisan key:generate
Paso 9  - consola: php artisan migrate

opcional poner información para probar la API
Paso 10 - consola: php artisan db:seed --class=ExampleDataPresentationSeed