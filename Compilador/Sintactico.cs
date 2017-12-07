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
        List<string> codigo = new List<string>();//faby
        List<string> tags = new List<string>();
        //int methodToken1 = -1;
        int methodConditionIndex = -1;
        string condCMPJMP = string.Empty;
        public Sintactico(Lexico lexico)
        {
            lex = lexico;
            tokens = lex.getTokens();
            validarCochineroLlaves();
            for (int i = 0; i < tokens.Count; i++)
            {
                getTipo(tokens.ElementAt(i), i);
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
                case "comparison":
                    validarComparacion(pos);
                    break;
                case "operator":
                    if (tok.lexema == "=") { validarComparacion(pos); }
                    break;
                case "metodo":
                    {
                        genMetodo(pos);
                        break;
                    }
            }
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
                    if (tipoVariable[index] != tipoVariable[index2])
                    {
                        errores.AddLast(error);
                    }//if
                    else
                    {
                        genCondicion(tokens.ElementAt(posicion-1), tokens.ElementAt(posicion+1));
                        methodConditionIndex = posicion;//indice de token de condicion del metodo
                    }//ffaaby
                }
                else
                {
                    if (!compararConstanteOString(tipoVariable[index], pos, 1)) { errores.AddLast(error); }
                }
            }

            if(tokens.ElementAt(pos - 1).tipo == "constant"|| tokens.ElementAt(pos - 1).tipo == "string")
            {
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
                ;
                if (tipo[tipo.IndexOf("identifier") + 1] != "corchete")
                {
                    variableDeclaradaCorrectamente = false;
                    errores.AddLast("Linea: " + tokens.ElementAt(pos + 1).linea + " Posicion: " + (tipo.IndexOf("identifier") + 1) + Environment.NewLine + "Error: al declarar una variable string es necesario definir su tamaño.");
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

        public void genMetodo(int pos)
        {
            switch(tokens.ElementAt(pos).lexema)
            {
                case "para":
                    { break; }
                case "si":
                    { break; }
                case "mientras":
                    {
                        string tag1 = genTag("#mientras");
                        string tag2 = genTag("#mientras");
                        //methodToken1 = pos;
                        if(pos+1 < tokens.Count() && tokens.ElementAt(pos+1).lexema=="(")
                        {
                        for (pos=pos+1; pos < tokens.Count && tokens.ElementAt(pos).lexema!=")"; pos++)//o hasta parenthesis?
                        {
                            getTipo(tokens.ElementAt(pos), pos);
                        }//for
                            string actualCondition = string.Empty;
                            actualCondition+=genCodAddJmpType((tokens.ElementAt(methodConditionIndex)).lexema) + tag1;
                            codigo.Add(actualCondition);
                            condCMPJMP += actualCondition;
                            codigo.Add(genCodAddJmpType("incondicional") + tag2);
                            codigo.Add(tag1);

                        if (tokens.ElementAt(pos+1).lexema=="{")
                        {

                        }//if
                        else
                        {
                                //asume que el ultimo elemento fue un parenthesis cerrado
                            int prevPos = pos;
                            for (pos = pos + 1; pos < tokens.Count && tokens.ElementAt(pos).linea == tokens.ElementAt(pos).linea+1; pos++)//o hasta parenthesis?
                            {
                               getTipo(tokens.ElementAt(pos), pos);
                            }//
                        }//sin corchete
                            
                            codigo.Add(tag2);
                        }//if open parenthesis
                        
                        getTipo(tokens.ElementAt(pos),pos);
                        break;
                    }//mientras
            }//switch
        }//validar

        public string genCodAddJmpType(String tok)
        {//esComparison
            switch (tok)
            {
                case ">":
                    {
                        return "JGT ";
                    }
                case "<":
                    {
                        return "JLT ";
                    }
                case ">=":
                    {
                        return "JGE ";
                    }
                case "<=":
                    {
                        return "JLE ";
                    }
                case "==":
                    {
                        return "JEQ ";
                    }
                case "!=":
                    {
                        return "JNE ";
                    }
                default:
                    {
                        return "JMP ";
                    }
            }//switch
        }//getCoddAddJmpType

        public string genTag(string x)
        {
            tags.Add(x+tags.Count().ToString());
            return tags.Last();
        }//genTag
        public void genCondicion(token valor1, token valor2)
        {
            condCMPJMP = "";
            genCodDoPushType(valor1);
            genCodDoPushType(valor2);
            codigo.Add("CMP");
            condCMPJMP += "CMP "+"\n";
        }//genCondicion
        public void genCodDoPushType(token valor)
        {
            if(valor.tipo== "constant")
            {
                if( (Convert.ToDouble(valor.lexema) % 1) == 0 && !valor.lexema.Contains("."))
                {
                    codigo.Add( "PUSHKI "+valor.lexema);
                    condCMPJMP += "PUSHKI " + valor.lexema+"\n";
                }//es int
                else
                {
                    codigo.Add("PUSHKD " + valor.lexema);
                    condCMPJMP+= "PUSHKD " + valor.lexema+"\n";
                }//es double
            }//es valor numerico
            else
            {
                if (valor.tipo == "string")
                {
                    if (valor.lexema.ElementAt(0) == 39)
                    {
                        codigo.Add( "PUSHKC " + valor.lexema);
                        condCMPJMP += "PUSHKC " + valor.lexema + "\n";
                    }//esChar
                    else
                    {
                        codigo.Add( "PUSHKS " + valor.lexema);
                        condCMPJMP += "PUSHKS " + valor.lexema + "\n";
                    }
                }//string o char
                else
                {
                    if(valor.tipo== "identifier")
                    {
                        
                        if(variables.Contains(valor.lexema))
                        {
                            int ind = variables.IndexOf(valor.lexema);
                            switch(tipoVariable.ElementAt(ind))
                            {
                                case "entero":
                                    {
                                        codigo.Add( "PUSHI " + valor.lexema);
                                        condCMPJMP += "PUSHI " + valor.lexema + "\n";
                                        break;
                                    }
                                case "palabra":
                                    {
                                        codigo.Add("PUSHS " + valor.lexema);
                                        condCMPJMP += "PUSHS " + valor.lexema + "\n";
                                        break;
                                    }
                                case "real":
                                    {
                                        codigo.Add( "PUSHD " + valor.lexema);
                                        condCMPJMP += "PUSHD " + valor.lexema + "\n";
                                        break;
                                    }
                                case "caracter":
                                    {
                                        codigo.Add( "PUSHC " + valor.lexema);
                                        condCMPJMP += "PUSHC " + valor.lexema + "\n";
                                        break;
                                    }
                            }//ver que tipo de variable es
                        }
                    }//if(valor.tipo== "identifier")
                }//not constant string or char
            }//else
           

        }//getCodGetPushType
    }//class sintatico
}//namespace compilador