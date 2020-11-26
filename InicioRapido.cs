using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace InicioRapido
{

    /*
     TODO
        
        - Investigar posibilidad de pasar datos (texto) a pastebin.
            - Hecho. Ampliar funcionalidad para poder sacar el texto de un fichero
        - Integrar variables de sistema en el lenguaje de script, como fecha (formato yyyymmdd), appdata.
       
        - Repasar "interfaz".
        
    -Funcionalidad inútil: Escribir en pantalla un string de una linea aleatoria de un fichero de texto
        - M97 ONombreficheroendirectorioappdata
        - Tiene que estar antes del primer N
        - El fichero del que se lee se establece en el momento de leer el cfg
        - El string aleatorio cambia cada vez que se accede a un menú


     */

    class InicioRapido
    {

        string Ruta_CarpetaAppData, NombreFichero_Opciones, RutaCompleta_Opciones;
        string String_FraseDelMomento; //usar variables públicas es cómodo

        string[] Array_ListaOpciones = new string[999];
        string[] Array_Acciones = new string[10]; //Establecemos un maximo de 10 acciones por opción (sin contar subopciones, que tendrán su propia función para ser leídas)
        string[] Array_Archivo = new string[999]; //establecemos un máximo de 999 lineas por archivo
        string[] Array_AtajoOpciones = new string[999]; ////establecemos un máximo de 999 atajos a opciones
        string[] Array_FrasesDelMomento;
        int[] Array_IndiceMenus = new int[999]; //establecemos un máximo de 999 menús

        int Int_NumeroOpciones, Int_AnchoVentanaFinal;
        int Int_NumeroAleatorio = -1;
        const int Int_AnchoVentanaInicial = 40;

        bool bool_EsSubmenu = false; //no me gusta que esta variable sea global, pero de momento es lo que toca

        [STAThread] //Necesario si queremos que funcione lo de copiar texto al portapapeles
        static void Main(string[] args)
        {
            InicioRapido IR = new InicioRapido();
            Console.SetWindowSize(40, 20);

            IR.InicializarVariables();

            

            IR.InicializarDirectorioCFG();
            IR.IndexarMenus();
            IR.LeerArchivoFrasesDelMomento();

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
                if (Array_Archivo[i] == "") { continue; }

                if (Array_Archivo[i].Substring(0, 1) == "N" && (Bool_PrimerMenuEncontrado == false))//si la primera letra es una N, y no hemos encontrado el comienzo del menú principal
                {
                    Array_IndiceMenus[0] = i; //almacenamos el numero de linea del primer menu
                    Bool_PrimerMenuEncontrado = true; //indicamos que ya sabemos donde empieza el menú principal

                }

                if (Array_Archivo[i].Substring(0, 1) == "P") //si la primera letra es una P
                {
                    Int_PosicionEnArrayIndiceMenus = Convert.ToInt32(Array_Archivo[i].Substring(1));

                    Array_IndiceMenus[Int_PosicionEnArrayIndiceMenus] = i; //le pasamos al array el numero de linea del menu, almacenando en la posición del array correspondiente

                }

            }


        }

        public void LeerArchivoFrasesDelMomento()
        {
            //Aquí buscaremos un string en el array que empiece por M97, la interpretaremos para saber qué archivo contiene los textos, comprobaremos si el archivo existe, y lo cargaremos en un array


            for (int i = 0; i < Array_Archivo.Length; i++)
            {
                if (Array_Archivo[i].Length < 3) { continue; }
                if (Array_Archivo[i].Substring(0, 3) == "M97")
                {
                    string NombreFichero_FrasesDelMomento = Array_Archivo[i].Substring(Array_Archivo[i].IndexOf('O') + 1);
                    string RutaCompleta_FrasesDelMomento = Ruta_CarpetaAppData + @"\" + NombreFichero_FrasesDelMomento;
                    try { Array_FrasesDelMomento = File.ReadAllLines(RutaCompleta_FrasesDelMomento); }
                    catch { Array_FrasesDelMomento[0] = null;
                    } 
                    break;

                }

            }


        }


        #endregion



        public void TodoMenu(int IndiceMenu) //Esta funcion toma el indice de menu, y lo lee, lo muestra, deja que selecciones, y ejecuta las acciones
        {

            if (IndiceMenu != 0)
            {
                bool_EsSubmenu = true;
            }
            else
            {
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

                if ((bool_EsSubmenu == true) && (Contador_N == 0))
                {
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
        public void LeerFraseDelMomento()
        {
            
            //comprobaremos si el array está vacio. Si lo está, metemos un placeholder en la variable y nos saltamos el resto con un return
            //En realidad me he asegurado de que haya al menos un elemento en el array, y que si no hay nada, le asigne el valor null.
            if (Array_FrasesDelMomento[0] == null)
            {
                String_FraseDelMomento = "Su frase aquí";
                return;
            }

            //En cualquier otro caso, seleccionamos un numero entre 0 y la longitud del array, y cogemos el string de ahí   

            Int_NumeroAleatorio = RNG_SIN(0, Array_FrasesDelMomento.Length-1);
            String_FraseDelMomento = Array_FrasesDelMomento[Int_NumeroAleatorio];
            
        }




        public void MostrarOpciones()
        {
            char[] Array_CaracteresFinTextoOpcion = new char[1] { '#' };
            int Contador_N = 0;

            Console.Clear();
            LeerFraseDelMomento();
            Console.WriteLine(String_FraseDelMomento);
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

            //Anda mira, un trocito de logica que decide el ancho de la ventana en función del largo de la frase
            if(String_FraseDelMomento.Length > Int_AnchoVentanaInicial){
                Int_AnchoVentanaFinal = String_FraseDelMomento.Length;
            }
            else
            {
                Int_AnchoVentanaFinal = Int_AnchoVentanaInicial;
            }

           
            Console.SetWindowSize(Int_AnchoVentanaFinal, Int_NumeroOpciones + 10); //Establecemos el tamaño de la ventana
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

                Entrada_OpcionSeleccionada = Console.ReadLine().ToLower(); //aunque introduzcamos mayúsuculas, lo convierte a minúsculas

                try { EntradaConvertida_OpcionSeleccionada = Convert.ToInt32(Entrada_OpcionSeleccionada); } //si es un int, es el número de la opción
                catch //si no, puede ser el atajo
                {
                    Contador_ComparacionListaOpcionesConAtajos = 0; //ponemos el contador a 0
                    foreach (string line in Array_AtajoOpciones)
                    {
                        if (line == Entrada_OpcionSeleccionada)
                        {
                            EntradaConvertida_OpcionSeleccionada = Contador_ComparacionListaOpcionesConAtajos;
                            break;
                        }
                        Contador_ComparacionListaOpcionesConAtajos++;
                    }

                    if (EntradaConvertida_OpcionSeleccionada == -1) { Error_ConvertirOpcionSeleccionada = true; } //si no ha habido coincidencia, activamso flag de error


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
            Array.Clear(Array_Acciones, 0, Array_Acciones.Length);



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

            foreach (string line in Array_Acciones)
            {
                if (line == null) { break; }
                M_Accion = line.Substring(0, 3);
                switch (M_Accion)
                {
                    case "M03":
                        string Param_M03 = line.Substring(line.IndexOf("F") + 1);
                        try { Process.Start(Param_M03); }
                        catch { Console.Write("Hubo un error al intentar ejecutar la acción."); Console.WriteLine("Comprueba que la orden está escrita correctamente en el script"); Console.ReadLine(); }//Esta funcion hace todo lo que tenía pensado inicialmente
                        break;

                    case "G04": //pausa de duración programada
                        int Tiempo_PausaMilisegundos = Convert.ToInt32(line.Substring(line.IndexOf("T") + 1));
                        try { Thread.Sleep(Tiempo_PausaMilisegundos); }
                        catch { Console.WriteLine("No se ha podido ejecutar la pausa"); Console.WriteLine("Comprueba que lo has escrito correctamente en el script"); Console.ReadLine(); }
                        break;

                    case "M98": //llamada a submenú
                        int NumeroMenu = Convert.ToInt32(line.Substring(line.IndexOf("P") + 1));
                        if (NumeroMenu == 0) { continue; } //si la llamada es al menú 0, entendemos que queremos ir al menú anterior. Así que salimos sin ejecutar
                        TodoMenu(NumeroMenu);
                        break;
                    case "M06": //meter datos al portapapeles
                        string Param_M06 = line.Substring(4);
                        Accion_M06(Param_M06);
                        break;
                }



            }


        }

        public void Accion_M06(string Param_M06)
        {
            string String_Switch = Param_M06.Substring(0, 1);
            switch (String_Switch)
            {
                case "T":
                    Clipboard.SetText(Param_M06.Substring(1));
                    break;
            }


        }
        #endregion

        #region Funciones auxiliares
        public int RNG_SIN(int min, int max) 
        {
            //Generamos un número aleatorio a partir de la semilla y los valores maximos y minimos
            double Double_Semilla = DateTime.Now.ToBinary();
            double Double_Temporal;

            Double_Temporal = Math.Abs(Math.Sin(Double_Semilla));
            Double_Temporal = Double_Temporal * (max - min) ;
            Double_Temporal = Double_Temporal + min;
            Double_Temporal = Math.Round(Double_Temporal);

            return Convert.ToInt32(Double_Temporal);


        }
        #endregion
    }
}
