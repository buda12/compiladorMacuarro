using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    class Sintactico
    {
        Lexico lex;
        LinkedList<token> tokens;
        LinkedList<string> errores = new LinkedList<string>();
        List<string> variables = new List<string>();
        List<string> tipoVariable = new List<string>();
        bool variableEsArreglo = false;
        public Sintactico(Lexico lexico)
        {
            lex = lexico;
            tokens = lex.getTokens();
            validarProgramaComienzaLlave();
            validarCochineroLlaves();
            for (int i = 0; i < tokens.Count; i++)
            {
                getTipo(tokens.ElementAt(i), i);
            }
        }

        public void validarProgramaComienzaLlave()
        {
            if (tokens.ElementAt(0).lexema != "{")
            {
                errores.AddLast("Linea: 1 Posicion: 1 \nError: el programa debe comenzar con {");
            }
        }

        public void getTipo(token tok, int pos)
        {
            switch (tok.tipo)
            {
                case "keyword":
                    validarDeclaracionVariables(pos);
                    break;
                case "identifier":
                    validarVariableExiste(tok.lexema, pos);
                    break;
                //case "comparison":
                    //validarComparacion(pos);
                    //break;
                case "operator":
                    if (tok.lexema == "=") { validarComparacion(pos); }
                    break;
                case "metodo":
                    validarMetodo(tok.lexema, pos);
                    break;
            }
        }

        public void validarMetodo(string metodo, int pos)
        {
            switch (metodo)
            {
                case "leer":
                case "imprimir":
                case "simprimir":
                    int i = pos + 2;
                    bool sigueComa = false;
                    while (tokens.ElementAt(i).lexema != ")")
                    {
                        i = hayTamañoMatriz(i);
                        if (tokens.ElementAt(i).lexema != ")")
                        {
                            if (sigueComa)
                            {
                                if (tokens.ElementAt(i).lexema != ",")
                                {
                                    errores.AddLast("Linea: " + tokens.ElementAt(i).linea + " Posicion: " + tokens.ElementAt(i).index + Environment.NewLine + "Error: en separacion de parametros. Debe de utilizar ,");
                                }
                            }
                            if (!sigueComa)
                            {
                                esDato(i, pos);
                            }
                            if (variableEsArreglo) //haz tu chochinero
                            {

                            }
                            variableEsArreglo = false;
                            sigueComa = !sigueComa;
                            i++;
                        }
                    }
                    break;
                case "si":
                case "mientras":
                    int j = pos + 2;
                    esDato(j, pos);
                    j++;
                    j = hayTamañoMatriz(j);
                    if (tokens.ElementAt(j).tipo != "comparison")
                    {
                        errores.AddLast("Linea: " + tokens.ElementAt(j).linea + " Posicion: " + tokens.ElementAt(j).index + Environment.NewLine + "Error: tipo de dato invalido. Debe de haber una comparacion");
                    }
                    if (tokens.ElementAt(j).tipo == "comparison")
                    {
                        validarComparacion(j);
                    }
                    esDato(j + 1, pos);
                    break;
                case "para":
                    bool hayError = false;
                    int index = pos + 2;
                    esAsignacion(index);
                    index += 3;
                    if (tokens.ElementAt(index).lexema != ";")
                    {
                        errores.AddLast("Linea: " + tokens.ElementAt(index).linea + " Posicion: " + tokens.ElementAt(index).index + Environment.NewLine + "Error: los parametros en van separados por ;.");
                    }
                    esComparacion(index + 1);
                    index++;
                    while (tokens.ElementAt(index).lexema != ";")
                    {
                        if (tokens.ElementAt(index).lexema == ")")
                        {
                            hayError = true;
                            errores.AddLast("Linea: " + tokens.ElementAt(index).linea + " Posicion: " + tokens.ElementAt(index).index + Environment.NewLine + "Error: falta separador ; para el tercer parametro de para.");
                            break;
                        }
                        index++;
                    }
                    if (!hayError)
                    {
                        esAsignacionOIncremento(index + 1);
                    }
                    break;
            }
        }

        public void esComparacion(int pos)
        {
            bool esComparacion = false;
            int index = pos;
            while (tokens.ElementAt(pos).lexema != ";")
            {
                if (tokens.ElementAt(pos).tipo == "comparison")
                {
                    esComparacion = true;
                    index = pos;
                }
                pos++;
            }
            if (esComparacion)
            {
                validarComparacion(index);
            }
            else
            {
                errores.AddLast("Linea: " + tokens.ElementAt(pos).linea + " Posicion: " + tokens.ElementAt(pos).index + Environment.NewLine + "Error: debe de haber una comparasion.");
            }
        }

        public void esAsignacionOIncremento(int pos)
        {
            if (tokens.ElementAt(pos).tipo != "identifier")
            {
                errores.AddLast("Linea: " + tokens.ElementAt(pos).linea + " Posicion: " + tokens.ElementAt(pos).index + Environment.NewLine + "Error: las asignaciones deben de ser a una variable.");
            }
            if (tokens.ElementAt(pos + 1).lexema != "=" && tokens.ElementAt(pos + 1).lexema != "+" && tokens.ElementAt(pos + 1).lexema != "-")
            {
                errores.AddLast("Linea: " + tokens.ElementAt(pos + 1).linea + " Posicion: " + tokens.ElementAt(pos + 1).index + Environment.NewLine + "Error: falta el operandor =.");
            }
            if (tokens.ElementAt(pos + 1).lexema == "+" || tokens.ElementAt(pos + 1).lexema == "-")
            {
                if (tokens.ElementAt(pos + 1).lexema != tokens.ElementAt(pos + 2).lexema)
                {
                    errores.AddLast("Linea: " + tokens.ElementAt(pos + 2).linea + " Posicion: " + tokens.ElementAt(pos + 2).index + Environment.NewLine + "Error: al hacer un incremento o decremento.");
                }
            }
            if (tokens.ElementAt(pos + 1).lexema == "=")
            {
                validarComparacion(pos + 1); //valida que la variable y su asigancion sean del mismo tipo de dato
            }
        }

        public void esAsignacion(int pos)
        {
            if (tokens.ElementAt(pos).tipo != "identifier")
            {
                errores.AddLast("Linea: " + tokens.ElementAt(pos).linea + " Posicion: " + tokens.ElementAt(pos).index + Environment.NewLine + "Error: las asignaciones deben de ser a una variable.");
            }
            if (tokens.ElementAt(pos + 1).lexema != "=")
            {
                errores.AddLast("Linea: " + tokens.ElementAt(pos).linea + " Posicion: " + tokens.ElementAt(pos).index + Environment.NewLine + "Error: falta el operandor =.");
            }
            validarComparacion(pos + 1); //valida que la variable y su asigancion sean del mismo tipo de dato
        }

        public void esDato(int pos, int posMetodo)
        {
            if (tokens.ElementAt(posMetodo).lexema == "leer")
            {
                if (tokens.ElementAt(pos).tipo != "identifier")
                {
                    errores.AddLast("Linea: " + tokens.ElementAt(pos).linea + " Posicion: " + tokens.ElementAt(pos).index + Environment.NewLine + "Error: tipo de dato invalido. Los parametros del metodo leer deben de ser variables");
                }
            }
            else
            {
                if (tokens.ElementAt(pos).tipo != "identifier" && tokens.ElementAt(pos).tipo != "string" && tokens.ElementAt(pos).tipo != "constant")
                {
                    errores.AddLast("Linea: " + tokens.ElementAt(pos).linea + " Posicion: " + tokens.ElementAt(pos).index + Environment.NewLine + "Error: tipo de dato invalido. Debe de ser string, numerico o alguna variable");
                }
            }
        }

        public int hayTamañoMatriz(int pos)
        {
            if (tokens.ElementAt(pos).lexema == "[")
            {
                while (tokens.ElementAt(pos).lexema != "]")
                {
                    pos++;
                }
                pos++;
                variableEsArreglo = true;
            }
            return pos;
        }

        public int getPosicion(string tipo, int pos, int orden)
        {
            while (tokens.ElementAt(pos).tipo != tipo)
            {
                pos += orden;
            }
            return pos;
        }

        public void validarComparacion(int pos)
        {
            int posicion = pos;
            string error = "Linea: " + tokens.ElementAt(pos).linea + " Posicion: " + tokens.ElementAt(pos).index + Environment.NewLine + "Error: comparasion invalida. Los argumentos son de diferente tipo de dato";

            if (tokens.ElementAt(pos - 1).tipo == "corchete")
            {
                posicion = getPosicion("corchete", pos-2, -1);
            }

            if (tokens.ElementAt(posicion - 1).tipo == "identifier")
            {
                int index = variables.IndexOf(tokens.ElementAt(posicion - 1).lexema);

                if (tokens.ElementAt(pos + 1).tipo == "identifier")
                {
                    int index2 = variables.IndexOf(tokens.ElementAt(pos + 1).lexema);
                    if (index >= 0 && index2 >= 0)
                    {
                        if (tipoVariable[index] != tipoVariable[index2])
                        {
                            errores.AddLast(error);
                        }
                    }
                }
                else
                {
                    if (!compararConstanteOString(tipoVariable[index], pos, 1)) { errores.AddLast(error); }
                }
            }

                if (tokens.ElementAt(pos - 1).tipo == "constant" || tokens.ElementAt(pos - 1).tipo == "string")
                {
                if (tokens.ElementAt(pos).lexema == "=")
                {
                    errores.AddLast("Linea: " + tokens.ElementAt(pos).linea + " Posicion: " + tokens.ElementAt(pos).index + Environment.NewLine + "Error: no se puede asignar valor de una variable a una constante.");
                }

                    if (tokens.ElementAt(pos + 1).tipo == "identifier")
                    {
                        int index = variables.IndexOf(tokens.ElementAt(pos + 1).lexema);
                        if (!compararConstanteOString(tipoVariable[index], pos, -1)) { errores.AddLast(error); }
                    }
                    else
                    {
                        if (tokens.ElementAt(pos - 1).tipo != tokens.ElementAt(pos + 1).tipo) { errores.AddLast(error); }
                    }
                }
        }

        public bool compararConstanteOString(string tipoDato, int pos, int orden)
        {
            switch (tipoDato)
            {
                case "entero":
                case "real":
                    if (tokens.ElementAt(pos + orden).tipo == "constant")
                    {
                        return true;
                    }
                    return false;
                case "palabra":
                case "caracter":
                    if (tokens.ElementAt(pos + orden).tipo == "string")
                    {
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        public void validarVariableExiste(string var, int pos)
        {
            if (!variables.Contains(var))
            {
                errores.AddLast("Linea: " + tokens.ElementAt(pos).linea + " Posicion: " + tokens.ElementAt(pos).index + Environment.NewLine + "Error: variable no declarada");
            }
        }

        public void validarDeclaracionVariables(int pos)
        {
            List<string> tipo = new List<string>();
            List<string> contenido = new List<string>();
            int posAux = pos;
            while (tokens.ElementAt(posAux).lexema != ";")
            {
                tipo.Add(tokens.ElementAt(posAux).tipo);
                contenido.Add(tokens.ElementAt(posAux).lexema);
                posAux++;
            }
            bool variableDeclaradaCorrectamente = true;
            if (tokens.ElementAt(pos + 1).tipo != "identifier" && tokens.ElementAt(pos + 1).lexema != "[")
            {
                variableDeclaradaCorrectamente = false;
                errores.AddLast("Linea: " + tokens.ElementAt(pos).linea + " Posicion: " + tokens.ElementAt(pos).index + Environment.NewLine + "Error: fallo al declarar una variable.");
            }
            if (tokens.ElementAt(pos).lexema == "palabra")
            {
                if (contenido[tipo.IndexOf("identifier") + 1] != "(")
                {
                    variableDeclaradaCorrectamente = false;
                    errores.AddLast("Linea: " + tokens.ElementAt(pos + 1).linea + " Posicion: " + (tipo.IndexOf("identifier") + 1) + Environment.NewLine + "Error: al declarar una variable string es necesario definir su tamaño con parentesis.");
                }
                if (contenido[tipo.IndexOf("identifier") + 2] != "entero" && tipo[tipo.IndexOf("identifier") + 2] != "constant")
                {
                    variableDeclaradaCorrectamente = false;
                    errores.AddLast("Linea: " + tokens.ElementAt(pos + 2).linea + " Posicion: " + (tipo.IndexOf("identifier") + 2) + Environment.NewLine + "Error: al declarar una variable string es necesario definir su tamaño con un entero.");
                }
            }
            if (variableDeclaradaCorrectamente)
            {
                for (int i = 0; i < tipo.Count; i++)
                {
                    if (tipo[i] == "identifier")
                    {
                        tipoVariable.Add(tokens.ElementAt(pos).lexema);
                        variables.Add(contenido[i]);
                    }
                }
            }
        }

        // aqui valido los tres tipos de corchete o lo que sea, los tres metodos son super redundantes es exactamente lo mismo solo que
        // comparo diferentes caracteres, si se les ocurre algo mejor fierro
        public void validarCochineroLlaves()
        {
            llaves();
            corchetes();
            parentesis();
        }



        public void llaves()
        {
            int parentesis = 0;
            bool empiezaParentesisA = false;
            bool hayParentesis = false;
            string ultimaLlaveApertura = string.Empty;
            int ultimaLlaveIndex = -1;

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens.ElementAt(i).lexema == "{")
                {
                    parentesis++;
                    if (!hayParentesis)
                        empiezaParentesisA = true;
                    hayParentesis = true;
                }
                if (getLastLlaveApertura(tokens.ElementAt(i).lexema) != "nel")
                {
                    ultimaLlaveApertura = getLastLlaveApertura(tokens.ElementAt(i).lexema);
                    ultimaLlaveIndex = i;
                }
                if (tokens.ElementAt(i).lexema == "}")
                {
                    parentesis--;
                    hayParentesis = true;
                    while (getLastLlaveApertura(tokens.ElementAt(ultimaLlaveIndex).lexema) == "nel" || ultimaLlaveIndex > 0)
                    {
                        ultimaLlaveApertura = getLastLlaveApertura(tokens.ElementAt(ultimaLlaveIndex).lexema);
                        ultimaLlaveIndex--;
                    }
                }
                if (ultimaLlaveApertura == "llave")
                {
                    if (tokens.ElementAt(i).lexema == ")" || tokens.ElementAt(i).lexema == "]")
                    {
                        errores.AddLast("Linea: " + tokens.ElementAt(i).linea + " Posicion: " + tokens.ElementAt(i).index + Environment.NewLine + " Error: orden equivocado de los caracteres de agrupacion [], {}, () ");
                    }
                }
            }
            if (parentesis < -1)
            {
                errores.AddLast(" Error: falta una llave de apertura {");
            }
            if (parentesis > 0)
            {
                errores.AddLast(" Error: falta llave de cierre }");
            }
            if (!empiezaParentesisA && hayParentesis)
            {
                errores.AddLast(" Error: debe comenzar con una llave de apertura {");
            }
        }

        public void corchetes()
        {
            int parentesis = 0;
            bool empiezaParentesisA = false;
            bool hayParentesis = false;
            string ultimaLlaveApertura = string.Empty;
            int ultimaLlaveIndex = -1;

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens.ElementAt(i).lexema == "[")
                {
                    parentesis++;
                    if (!hayParentesis)
                        empiezaParentesisA = true;
                    hayParentesis = true;
                }
                if (getLastLlaveApertura(tokens.ElementAt(i).lexema) != "nel")
                {
                    ultimaLlaveApertura = getLastLlaveApertura(tokens.ElementAt(i).lexema);
                    ultimaLlaveIndex = i;
                }
                if (tokens.ElementAt(i).lexema == "]")
                {
                    parentesis--;
                    hayParentesis = true;
                    while (getLastLlaveApertura(tokens.ElementAt(ultimaLlaveIndex).lexema) == "nel" || ultimaLlaveIndex > 0)
                    {
                        ultimaLlaveApertura = getLastLlaveApertura(tokens.ElementAt(ultimaLlaveIndex).lexema);
                        ultimaLlaveIndex--;
                    }
                }
                if (ultimaLlaveApertura == "corchete")
                {
                    if (tokens.ElementAt(i).lexema == ")" || tokens.ElementAt(i).lexema == "}")
                    {
                        errores.AddLast("Linea: " + tokens.ElementAt(i).linea + " Posicion: " + tokens.ElementAt(i).index + Environment.NewLine + " Error: orden equivocado de los caracteres de agrupacion [], {}, () ");
                    }
                }
            }
            if (parentesis < -1)
            {
                errores.AddLast(" Error: falta un corchete de apertura [");
            }
            if (parentesis > 0)
            {
                errores.AddLast(" Error: falta corchete de cierre ]");
            }
            if (!empiezaParentesisA && hayParentesis)
            {
                errores.AddLast(" Error: debe comenzar con un corchete de apertura [");
            }
        }

        public void parentesis()
        {
            int parentesis = 0;
            bool empiezaParentesisA = false;
            bool hayParentesis = false;
            string ultimaLlaveApertura = string.Empty;
            int ultimaLlaveIndex = -1;

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens.ElementAt(i).lexema == "(")
                {
                    parentesis++;
                    if (!hayParentesis)
                        empiezaParentesisA = true;
                    hayParentesis = true;
                }
                if (getLastLlaveApertura(tokens.ElementAt(i).lexema) != "nel")
                {
                    ultimaLlaveApertura = getLastLlaveApertura(tokens.ElementAt(i).lexema);
                    ultimaLlaveIndex = i - 1;
                }
                if (tokens.ElementAt(i).lexema == ")")
                {
                    parentesis--;
                    hayParentesis = true;
                    while (getLastLlaveApertura(tokens.ElementAt(ultimaLlaveIndex).lexema) == "nel" || ultimaLlaveIndex > 0)
                    {
                        ultimaLlaveApertura = getLastLlaveApertura(tokens.ElementAt(ultimaLlaveIndex).lexema);
                        ultimaLlaveIndex--;
                    }

                }
                if (ultimaLlaveApertura == "parentesis")
                {
                    if (tokens.ElementAt(i).lexema == "]" || tokens.ElementAt(i).lexema == "}")
                    {
                        errores.AddLast("Linea: " + tokens.ElementAt(i).linea + " Posicion: " + tokens.ElementAt(i).index + Environment.NewLine + " Error: orden equivocado de los caracteres de agrupacion [], {}, () ");
                    }
                }
                if (tokens.ElementAt(i).tipo == "metodo" && tokens.ElementAt(i + 1).tipo != "corchete" && tokens.ElementAt(i).lexema != "sino")
                {
                    errores.AddLast(" Error: despues de la palabra reservada " + tokens.ElementAt(i).lexema + " va (");
                }
            }
            if (parentesis < -1)
            {
                errores.AddLast(" Error: falta un parentesis de apertura (");
            }
            if (parentesis > 0)
            {
                errores.AddLast(" Error: falta parentesis de cierre )");
            }
            if (!empiezaParentesisA && hayParentesis)
            {
                errores.AddLast(" Error: debe comenzar con un parentesis de apertura (");
            }
        }

        public string getLastLlaveApertura(string caracter)
        {
            string llave = "nel";

            switch (caracter)
            {
                case "[":
                    llave = "corchete";
                    break;
                case "{":
                    llave = "llave";
                    break;
                case "(":
                    llave = "parentesis";
                    break;
            }

            return llave;
        }

        public void imprimir()
        {
            if (errores.Count > 0)
            {
                Console.WriteLine("los errores son:");
            }
            else
            {
                Console.WriteLine("programa compilado con exito!");
            }
            for (int i = 0; i < errores.Count; i++)
            {
                Console.WriteLine(errores.ElementAt(i));
            }//for(int i=0; i<tokens.Count; i++)
        }//print
    }
}