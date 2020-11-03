using System;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace InicioRapido
{

    /*
     TODO

        - Pensar otras acciones que no sean sólo "ejecutar esto en cmd".
        - Repasar "interfaz".
        - Implementar subopciones, que son opciones contenidas en otro archivo. Identificados con Oxxxx, llamados con M98 Oxxxx
        - Implementar subacciones, que son acciones escritas después del M30. Identificadas con Pxxxx, llamadas con M98 Pxxxx

    Change log:
        2020 11 3
            - Añadida gestión de errores a la hora de ejecutar acciones
            - Añadida acción G04 Tx, donde x indica un tiempo de pausa en milisegundos
            - Añadidas regiones al código
     */

    class InicioRapido
    {

        string Ruta_CarpetaAppData, NombreFichero_Opciones, RutaCompleta_Opciones;
        string[] Array_ListaOpciones = new String[999];
        string[] Array_Acciones = new string[10]; //Establecemos un maximo de 10 acciones por opción (sin contar subopciones, que tendrán su propia función para ser leídas)
        string[] Array_Archivo = new string[999]; //establecemos un máximo de 999 lineas por archivo

        static void Main(string[] args)
        {
            InicioRapido IR = new InicioRapido();
            Console.SetWindowSize(40, 15);

            IR.InicializarVariables();
            IR.InicializarDirectorioCFG();
            IR.InterpretarCFG_LeerN();

            while (true)
            {
                IR.MostrarOpciones();
                IR.SeleccionarOpcion();
                IR.EjecutarAcciones();
            }

        }

        //*********************************************************************************************************************************************************

        #region Asignamos valores a variables, comprobamos que existen los archivos y directorios de appdata
        public void InicializarVariables()
        {

            Ruta_CarpetaAppData = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)) + @"\OKR\InicioRapido";   //Carpeta en appdata

            NombreFichero_Opciones = @"O0000.txt";                                                 //Nombre del fichero
            RutaCompleta_Opciones = (Ruta_CarpetaAppData + @"\" + NombreFichero_Opciones);      //Ruta completa del fichero


        }

        public void InicializarDirectorioCFG()
        {
            bool FilesWereCreated = false;

            if (File.Exists(RutaCompleta_Opciones) == false)
            {

                Directory.CreateDirectory(Ruta_CarpetaAppData);
                File.Create(RutaCompleta_Opciones);

                Console.WriteLine("Se ha creado el archivo {0}", RutaCompleta_Opciones);
                Console.WriteLine("En él puedes introducir nombres de opciones y rutas");
                Console.ReadLine();
                FilesWereCreated = true;
            }

            if (FilesWereCreated) { Environment.Exit(0); }
            
        }
        #endregion

        #region Leemos el archivo script principal

        public void InterpretarCFG_LeerN()
        {
            Array_Archivo = File.ReadAllLines(RutaCompleta_Opciones);
            string String_LeerNIf;
            int Contador_N = 0;
            int Contador_Linea = 0;

            foreach (string line in Array_Archivo)
            {

                String_LeerNIf = line;
                if (String_LeerNIf == "") { String_LeerNIf = ";"; } 


                if (String_LeerNIf.Substring(0, 1) == "N")
                {
                    Array_ListaOpciones[Contador_N] = String_LeerNIf.Substring(1) + " L" + Contador_Linea;
                    Contador_N++;
                }
                else
                {

                    if (String_LeerNIf == "M30")
                    {
                        Array_ListaOpciones[Contador_N] = String_LeerNIf + " L" + Contador_Linea;
                        break;
                    }

                }
                Contador_Linea++;
            }

        }
        #endregion

        #region Interfaz; mostramos opciones, logica de seleccion, lectura de acciones en opción
        public void MostrarOpciones()
        {
            int Contador_N = 0;

            Console.Clear();
            Console.WriteLine("Lista de opciones");
            Console.WriteLine("");
            Console.WriteLine("Nº|Nombre");
            Console.WriteLine("");

            foreach (string line in Array_ListaOpciones)
            {
                if (line.Substring(0, 3) == "M30")
                {
                    Console.WriteLine("");
                    Console.WriteLine(line.Substring(0, line.LastIndexOf('L')));
                    break;
                }
                else
                {
                    Console.WriteLine(Contador_N + line.Substring(0, line.LastIndexOf('L')));
                    Contador_N++;
                }

            }

        }

        public void SeleccionarOpcion() //tomamos input, nos dirigimos a la línea correspondiente, se la pasamos al array "Array_Acciones"
        {
            bool Error_ConvertirOpcionSeleccionada;
            string Entrada_OpcionSeleccionada;
            int EntradaConvertida_OpcionSeleccionada = -1;
            int Indice_OpcionSeleccionadaEnArrayArchivo;


            do
            {
                Error_ConvertirOpcionSeleccionada = false;

                Entrada_OpcionSeleccionada = Console.ReadLine();

                try { EntradaConvertida_OpcionSeleccionada = Convert.ToInt32(Entrada_OpcionSeleccionada); }
                catch
                {
                    Error_ConvertirOpcionSeleccionada = true;
                    Console.WriteLine("Introduce un número");

                }

            } while (Error_ConvertirOpcionSeleccionada == true);


            //primero sacamos el numero de linea en el que empieza la opción (indice 0)
            //si, eso es lo que hace la linea a continuación; lee el numero tras la L en el string del array de opciones
            //se recomienda separar la linea en sus funciones basicas por si quieres entenderla
            Indice_OpcionSeleccionadaEnArrayArchivo = Convert.ToInt32(Array_ListaOpciones[EntradaConvertida_OpcionSeleccionada].Substring((Array_ListaOpciones[EntradaConvertida_OpcionSeleccionada].LastIndexOf('L')) + 1));
            LeerAccionesDeOpcion(Indice_OpcionSeleccionadaEnArrayArchivo);


        }

        public void LeerAccionesDeOpcion(int IndiceDeOpcionEnArray)
        {
            int Contador_ArrayOpciones = 0;

            //antes de pasar nada al array, tenemos que vaciarlo
            foreach (string field in Array_Acciones)
            {
                Array_Acciones[Contador_ArrayOpciones] = null;
                Contador_ArrayOpciones++;

            }

            //leemos las lineas de acción del Array_Archivo a partir de la indicada por Indice_OpcionSeleccionadaEnArrayArchivo
            int Contador_EntradaArrayAcciones = 0;
            int Contador_SalidaArrayArchivo = IndiceDeOpcionEnArray + 1;
            string String_DoWhileLecturaArrayArchivo; //20201103 Si dentro de un mes miras esto y no sabes lo que es, dale la razón a yozi

            do
            {
                String_DoWhileLecturaArrayArchivo = Array_Archivo[Contador_SalidaArrayArchivo];

                if (String_DoWhileLecturaArrayArchivo == "") { String_DoWhileLecturaArrayArchivo = ";"; }

                if (String_DoWhileLecturaArrayArchivo.Substring(0, 1) == "N" || String_DoWhileLecturaArrayArchivo == "M30") { break; } //salimos si encontramos una N o un M30

                if (String_DoWhileLecturaArrayArchivo.Substring(0, 1) == "M" || String_DoWhileLecturaArrayArchivo.Substring(0, 1) == "G")
                {
                    Array_Acciones[Contador_EntradaArrayAcciones] = String_DoWhileLecturaArrayArchivo;
                    Contador_EntradaArrayAcciones++;
                } //solo metemos acciones en el array, nos ahorramos las lineas que no lo son             

                Contador_SalidaArrayArchivo++;

            } while (true);

        }
        #endregion

        #region Ejecución de acciones de la opción seleccionada
        public void EjecutarAcciones() //ejecutamos las opciones almacenadas en el array_acciones
        {
            string M_Accion;
           
            //leemos cada string del array hasta llegar a un null
                //interpretamos las líneas

            foreach(string line in Array_Acciones)
            {
                if (line == null) { break; }
                M_Accion = line.Substring(0, 3);
                switch (M_Accion)
                {
                    case "M03":
                        string Argumento_Accion = line.Substring(line.IndexOf("F") + 1);
                        try { Process.Start(Argumento_Accion); }
                        catch { Console.Write("Hubo un error al intentar ejecutar la acción."); Console.WriteLine("Comprueba que la orden está escrita correctamente en el script"); Console.ReadLine(); }//Esta funcion hace todo lo que tenía pensado inicialmente
                        break;
                    case "G04": //pausa de duración programada
                        int Tiempo_PausaMilisegundos = Convert.ToInt32(line.Substring(line.IndexOf("T")+1));
                        try { Thread.Sleep(Tiempo_PausaMilisegundos); }
                        catch { Console.WriteLine("No se ha podido ejecutar la pausa"); Console.WriteLine("Comprueba que lo has escrito correctamente en el script"); Console.ReadLine(); }
                        break;
                }



            }
            
        
        }
        #endregion
    }

}
