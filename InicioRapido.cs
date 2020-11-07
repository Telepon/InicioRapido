using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net;

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
       
         - Repasar "interfaz".


     */

    class InicioRapido
    {

        string Ruta_CarpetaAppData, NombreFichero_Opciones, RutaCompleta_Opciones;

        string[] Array_ListaOpciones = new string[999];
        string[] Array_Acciones = new string[10]; //Establecemos un maximo de 10 acciones por opción (sin contar subopciones, que tendrán su propia función para ser leídas)
        string[] Array_Archivo = new string[999]; //establecemos un máximo de 999 lineas por archivo
        string[] Array_AtajoOpciones = new string[999]; ////establecemos un máximo de 999 atajos a opciones
        int[] Array_IndiceMenus = new int[999]; //establecemos un máximo de 999 menús

        int Int_NumeroOpciones;

        bool bool_EsSubmenu = false; //no me gusta que esta variable sea global, pero de momento es lo que toca

        static void Main(string[] args)
        {
            InicioRapido IR = new InicioRapido();
            Console.SetWindowSize(40, 20);

            IR.InicializarVariables();
            IR.InicializarDirectorioCFG();
            IR.IndexarMenus();

            do
            {
                IR.TodoMenu(0);
            } while (true);
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

        public void IndexarMenus()
        {
            bool Bool_PrimerMenuEncontrado = false;
            int Int_PosicionEnArrayIndiceMenus;

            Array_Archivo = File.ReadAllLines(RutaCompleta_Opciones);

            for (int i = 0; i < Array_Archivo.Length; i++)
            {
                if(Array_Archivo[i] == "") { continue; }

                if( Array_Archivo[i].Substring(0,1) == "N" && (Bool_PrimerMenuEncontrado == false) )//si la primera letra es una N, y no hemos encontrado el comienzo del menú principal
                {
                    Array_IndiceMenus[0] = i; //almacenamos el numero de linea del primer menu
                    Bool_PrimerMenuEncontrado = true; //indicamos que ya sabemos donde empieza el menú principal

                }

                if(Array_Archivo[i].Substring(0,1) == "P") //si la primera letra es una P
                {
                    Int_PosicionEnArrayIndiceMenus = Convert.ToInt32(Array_Archivo[i].Substring(1));

                    Array_IndiceMenus[Int_PosicionEnArrayIndiceMenus] = i; //le pasamos al array el numero de linea del menu, almacenando en la posición del array correspondiente

                }

            }


        }

       
        #endregion

        

        public void TodoMenu(int IndiceMenu) //Esta funcion toma el indice de menu, y lo lee, lo muestra, deja que selecciones, y ejecuta las acciones
        {

            if (IndiceMenu != 0) {
                bool_EsSubmenu = true; 
            } else {
                bool_EsSubmenu = false; 
            }
            
                LeerMenu(IndiceMenu);
                MostrarOpciones();
                SeleccionarOpcion();
                EjecutarAcciones();


        }

        #region Interfaz; leemos menu, mostramos opciones, logica de seleccion, lectura de acciones en opción

        public void LeerMenu(int IndiceMenu)
        {
            //Al leer un menú, leemos desde una posición que nos indicará el array de indices de menu
            //Y leemos hasta que vemos un M99 o un M30

            int int_PosicionInicial = Array_IndiceMenus[IndiceMenu];

            int Contador_N = 0;

            //tenemos que vaciar el array de atajos antes de nada
            Array.Clear(Array_AtajoOpciones, 0, Array_AtajoOpciones.Length);
            //y el array de opciones
            Array.Clear(Array_ListaOpciones, 0, Array_ListaOpciones.Length);
            

            for (int i = int_PosicionInicial; i < Array_Archivo.Length; i++)
            {
                //Si es un submenú, haremos que la primera opción sea vovler hacia atrás
                //Lo metemos en la posición 0 del array lista de opciones
                //La opción de volver hacia atrás es M98 P0

                if ((bool_EsSubmenu == true) && (Contador_N == 0)) {
                    Array_ListaOpciones[0] = " <-- Volver #";
                    Contador_N++;
                }

                if (Array_Archivo[i] == "") { continue; } //ignoramos lineas vacías


                if (Array_Archivo[i].Substring(0, 1) == "N") //si empieza por N, es una opción
                {
                    Array_ListaOpciones[Contador_N] = Array_Archivo[i].Substring(1) + " L" + i; //almacenamos la opción con su numero de linea

                    if (Array_Archivo[i].Contains("#"))
                    {
                        if ((Array_Archivo[i].Substring(Array_Archivo[i].IndexOf("#") + 1)) != "") //evitamos meter en el array si no hay atajo especificado
                        {

                            if (Array.IndexOf(Array_AtajoOpciones, (Array_Archivo[i].Substring(Array_Archivo[i].IndexOf("#") + 1))) == -1)
                            {
                                Array_AtajoOpciones[Contador_N] = Array_Archivo[i].Substring(Array_Archivo[i].IndexOf("#") + 1);
                            }
                            else
                            {

                                Console.WriteLine("Atajos repetidos en el mismo menú\nComprueba el script");
                                Console.ReadLine();
                                Environment.Exit(0);

                            }
                        }
                    }
                    else
                    {
                        Array_AtajoOpciones[Contador_N] = null;
                    }
                    Contador_N++; //cuenta opciones
                }


                if (Array_Archivo[i] == "M30")
                {
                    Array_ListaOpciones[Contador_N] = Array_Archivo[i] + " L" + i;
                    break;
                }
                if (Array_Archivo[i] == "M99")
                {
                    Array_ListaOpciones[Contador_N] = Array_Archivo[i] + " L" + i;
                    break;
                }

            }

            

        }
        public void MostrarOpciones()
        {
            char[] Array_CaracteresFinTextoOpcion = new char[1] {'#'};
            int Contador_N = 0;

            Console.Clear();
            Console.WriteLine("Lista de opciones");
            Console.WriteLine("");
            Console.WriteLine("Nº|Nombre");

            foreach (string line in Array_ListaOpciones)
            {
                if (line.Substring(0, 3) == "M30" || line.Substring(0, 3) == "M99")
                {
                    Console.WriteLine("\n");
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

            Console.SetWindowSize(40, Int_NumeroOpciones + 10); //Establecemos el tamaño de la ventana
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

            if (bool_EsSubmenu == true && EntradaConvertida_OpcionSeleccionada == 0) //Si estamos en un submenú, y la entrada es 0, nos saltamos la conversión de entrada
            {
                 Array_Acciones[0] = "M98 P0";  //y metemos en el array de acciones la llamada a P0
                //al salir de este if, vamos directamente a ejecutar acciones
            }
            else
            {
                //primero sacamos el numero de linea en el que empieza la opción (indice 0)
                //si, eso es lo que hace la linea a continuación; lee el numero tras la L en el string del array de opciones
                //se recomienda separar la linea en sus funciones basicas por si quieres entenderla

                Indice_OpcionSeleccionadaEnArrayArchivo = Convert.ToInt32(Array_ListaOpciones[EntradaConvertida_OpcionSeleccionada].Substring((Array_ListaOpciones[EntradaConvertida_OpcionSeleccionada].LastIndexOf('L')) + 1));

                LeerAccionesDeOpcion(Indice_OpcionSeleccionadaEnArrayArchivo);

            }

        }

        public void LeerAccionesDeOpcion(int IndiceDeOpcionEnArray)
        {

            //antes de pasar nada al array, tenemos que vaciarlo
            Array.Clear(Array_Acciones,0,Array_Acciones.Length);

           

            //leemos las lineas de acción del Array_Archivo a partir de la indicada por Indice_OpcionSeleccionadaEnArrayArchivo
            int Contador_EntradaArrayAcciones = 0;
            int Contador_SalidaArrayArchivo = IndiceDeOpcionEnArray + 1;
            string String_DoWhileLecturaArrayArchivo; //20201103 Si dentro de un mes miras esto y no sabes lo que es, dale la razón a yozi

            do
            {
                String_DoWhileLecturaArrayArchivo = Array_Archivo[Contador_SalidaArrayArchivo];
               

                if (String_DoWhileLecturaArrayArchivo == "") { String_DoWhileLecturaArrayArchivo = ";"; }

                if (String_DoWhileLecturaArrayArchivo.Substring(0, 1) == "N" || String_DoWhileLecturaArrayArchivo == "M30" || String_DoWhileLecturaArrayArchivo == "M99") { break; } //salimos si encontramos una N o un M30

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

                    case "M98": //llamada a submenú
                        int NumeroMenu = Convert.ToInt32(line.Substring(line.IndexOf("P") + 1));
                        if (NumeroMenu == 0) { continue; } //si la llamada es al menú 0, entendemos que queremos ir al menú anterior. Así que salimos sin ejecutar
                        TodoMenu(NumeroMenu);
                        break;
                }



            }
            
        
        }
        #endregion
    }

}
