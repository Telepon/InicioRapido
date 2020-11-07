using System;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace InicioRapido
{

    /*
     TODO
        
        - Añadir un parametro de acceso rápido para las opciones, para acceder mediante cadena de texto. Esto precisrá un array - DONE
            - Revisar con tiempo y estructurar para que sea escalable a subopciones.
            - Añadir una comprobación de que no haya atajos repetidos. -- DONE
            - Que el color del acceso rápido sea configurable.

        - Investigar posibilidad de pasar datos (texto) a pastebin.
        - Integrar variables de sistema en el lenguaje de script, como fecha (formato yyyymmdd), appdata.
       
        - Implementar submenús, que son listas de opciones que no son la principal, y se pueden llamar desde una opción, mediante M98.
            - Lo ideal es poder tenerlas tanto en archivos separados como en el mismo archivo
            - En el caso del mismo archivo, la llamada será M98 Pxxxx.
            - En el caso de un archivo diferente, la llamada será M98 Oxxxx
            - La implementación reusará código, pero en principio no usaremos la misma función, ya que emplearemos un array diferente (en principio)   
    
        - Repasar "interfaz".

    Change log:
        2020 11 3
            - Añadida gestión de errores a la hora de ejecutar acciones
            - Añadida acción G04 Tx, donde x indica un tiempo de pausa en milisegundos
            - Añadidas regiones al código

        2020 11 4
            - Añadida la opción de especificar accesos rapidos a las opciones.
     */

    class InicioRapido
    {

        string Ruta_CarpetaAppData, NombreFichero_Opciones, RutaCompleta_Opciones;
        string[] Array_ListaOpciones = new string[999];
        string[] Array_Acciones = new string[10]; //Establecemos un maximo de 10 acciones por opción (sin contar subopciones, que tendrán su propia función para ser leídas)
        string[] Array_Archivo = new string[999]; //establecemos un máximo de 999 lineas por archivo
        string[] Array_AtajoOpciones = new string[999]; ////establecemos un máximo de 999 atajos a opciones

        int Int_NumeroOpciones;


        static void Main(string[] args)
        {
            InicioRapido IR = new InicioRapido();
            Console.SetWindowSize(35, 20);

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

            for (int i = 0; i < Array_Archivo.Length; i++)
            {
                
                String_LeerNIf = Array_Archivo[i];
                if (String_LeerNIf == "") { continue; } //cambiamos lineas vacias a ;


                if (String_LeerNIf.Substring(0, 1) == "N")//si empieza por N, es una opción
                {
                    Array_ListaOpciones[Contador_N] = String_LeerNIf.Substring(1) + " L" + i;

                    if (String_LeerNIf.Contains("#"))
                    {
                        if (Array.IndexOf(Array_AtajoOpciones, (String_LeerNIf.Substring(String_LeerNIf.IndexOf("#") + 1))) == -1)
                        {
                            Array_AtajoOpciones[Contador_N] = String_LeerNIf.Substring(String_LeerNIf.IndexOf("#") + 1);
                        }
                        else
                        {

                            Console.WriteLine("Atajos repetidos en el mismo menú\nComprueba el script");
                            Console.ReadLine();
                            Environment.Exit(0);

                        }
                    }
                    else
                    {
                        Array_AtajoOpciones[Contador_N] = null;
                    }
                    Contador_N++; //cuenta opciones
                }
                

                if (String_LeerNIf == "M30")
                {
                     Array_ListaOpciones[Contador_N] = String_LeerNIf + " L" + i;
                     break;
                }

                }
            

        }
        #endregion

        #region Interfaz; mostramos opciones, logica de seleccion, lectura de acciones en opción
        public void MostrarOpciones()
        {
            char[] Array_CaracteresFinTextoOpcion = new char[2] {'L', '#'};
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
                    Console.WriteLine(line.Substring(0, 3));                    
                    break;
                }
                else
                {
                    
                    Console.Write(Environment.NewLine + Contador_N + line.Substring(0, line.IndexOfAny(Array_CaracteresFinTextoOpcion)));
                    
                    Console.ForegroundColor = ConsoleColor.Red; //Letras rojas
                    Console.Write('#' + Array_AtajoOpciones[Contador_N]);                     
                    Console.ForegroundColor = ConsoleColor.Gray; //Letras grises, color por defecto
                    Contador_N++;
                }

            }
            Int_NumeroOpciones = Contador_N;
        }

        public void SeleccionarOpcion() //tomamos input, nos dirigimos a la línea correspondiente, se la pasamos al array "Array_Acciones"
        {
            bool Error_ConvertirOpcionSeleccionada;
            string Entrada_OpcionSeleccionada;
            int EntradaConvertida_OpcionSeleccionada = -1;
            int Indice_OpcionSeleccionadaEnArrayArchivo = -1;
            int Contador_ComparacionListaOpcionesConAtajos;


            do
            {
                Error_ConvertirOpcionSeleccionada = false;

                Entrada_OpcionSeleccionada = Console.ReadLine();

                try { EntradaConvertida_OpcionSeleccionada = Convert.ToInt32(Entrada_OpcionSeleccionada); } //si es un int, es el número de la opción
                catch //si no, puede ser el atajo
                {
                    Contador_ComparacionListaOpcionesConAtajos = 0; //ponemos el contador a 0
                    foreach (string line in Array_AtajoOpciones)
                    {
                        if (line == Entrada_OpcionSeleccionada) {
                            EntradaConvertida_OpcionSeleccionada = Contador_ComparacionListaOpcionesConAtajos;
                            break; 
                        }
                        Contador_ComparacionListaOpcionesConAtajos++;
                    }

                    if(EntradaConvertida_OpcionSeleccionada == -1){Error_ConvertirOpcionSeleccionada = true;} //si no ha habido coincidencia, activamso flag de error


                }

                if (Int_NumeroOpciones <= EntradaConvertida_OpcionSeleccionada) { Error_ConvertirOpcionSeleccionada = true; } //si el número pasado no está entre las opciones, activamos flag de error
                
                if (Error_ConvertirOpcionSeleccionada) { Console.WriteLine("Introduce un número de opción o un atajo válido."); }

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
