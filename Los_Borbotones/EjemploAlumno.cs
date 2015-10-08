using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;

namespace Los_Borbotones
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploAlumno : TgcExample
    {
        GameManager gameManager;

        /// <summary>
        /// Categor�a a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el �rbol de la derecha de la pantalla.
        /// </summary>
        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        /// <summary>
        /// Completar nombre del grupo en formato Grupo NN
        /// </summary>
        public override string getName()
        {
            return "Los Borbotones";
        }

        /// <summary>
        /// Completar con la descripci�n del TP
        /// </summary>
        public override string getDescription()
        {
            return @"Sniper - El objetivo del juego es sobrevivir la mayor cantidad de tiempo posible y sumar puntos, el juego termina cuando el jugador es alcanzado por los enemigos y pierde toda su vida.
                Presionar L para capturar el mouse. WASD para moverse. L-Shift Para correr. Click izqierdo para disparar, derecho para hacer zoom";
        }

        /// <summary>
        /// M�todo que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aqu� todo el c�digo de inicializaci�n: cargar modelos, texturas, modifiers, uservars, etc.
        /// Borrar todo lo que no haga falta
        /// </summary>
        public override void init()
        {
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;

            ///////////////USER VARS//////////////////

            //Crear una UserVar
            GuiController.Instance.UserVars.addVar("N Vegetacion Visible");

            //Cargar valor en UserVar
            GuiController.Instance.UserVars.setValue("N Vegetacion Visible", 0);

            //Crear una UserVar
            GuiController.Instance.UserVars.addVar("N Sub-terrenos Visibles", 0);

            //Cargar valor en UserVar
            GuiController.Instance.UserVars.setValue("N Sub-terrenos Visibles", 0);

            ///////////////MODIFIERS//////////////////

            GuiController.Instance.Modifiers.addBoolean("DrawBoundingBoxes", "Renderizar BoundingBoxes", false);

            GuiController.Instance.Modifiers.addBoolean("Invincibility", "Activar invencibilidad", false);

            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("weaponRotation", 0, 2f * (float)Math.PI, 0.1f);
            GuiController.Instance.Modifiers.addFloat("FlySpeed", 0, 1000, 0);

            //Crear un modifier para un ComboBox con opciones
            //string[] opciones = new string[]{"opcion1", "opcion2", "opcion3"};
            //GuiController.Instance.Modifiers.addInterval("valorIntervalo", opciones, 0);

            //Crear un modifier para modificar un v�rtice
            GuiController.Instance.Modifiers.addVertex3f("weaponOffset", new Vector3(-10, -20, -10), new Vector3(10, 10, 10), new Vector3(5f, -10.2f, 0.8f));

            //Creacion del Game Manager
            gameManager = GameManager.Instance;

            gameManager.Init();
        }


        /// <summary>
        /// M�todo que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aqu� todo el c�digo referido al renderizado.
        /// Borrar todo lo que no haga falta
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el �ltimo frame</param>
        public override void render(float elapsedTime)
        {
            //Device de DirectX para renderizar
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Obtener valores de Modifiers
            //string opcionElegida = (string)GuiController.Instance.Modifiers["valorIntervalo"];

            gameManager.Update(elapsedTime);
            gameManager.Render(elapsedTime);
        }

        /// <summary>
        /// M�todo que se llama cuando termina la ejecuci�n del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {
            gameManager.close();
        }

    }
}
