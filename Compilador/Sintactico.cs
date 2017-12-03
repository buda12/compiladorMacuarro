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

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens.ElementAt(i).lexema == "{")
                {
                    parentesis++;
                    if (!hayParentesis)
                        empiezaParentesisA = true;
                    hayParentesis = true;
                }
                if (tokens.ElementAt(i).lexema == "}")
                {
                    parentesis--;
                    hayParentesis = true;
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

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens.ElementAt(i).lexema == "[")
                {
                    parentesis++;
                    if (!hayParentesis)
                        empiezaParentesisA = true;
                    hayParentesis = true;
                }
                if (tokens.ElementAt(i).lexema == "]")
                {
                    parentesis--;
                    hayParentesis = true;
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

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens.ElementAt(i).lexema == "(")
                {
                    parentesis++;
                    if (!hayParentesis)
                        empiezaParentesisA = true;
                    hayParentesis = true;
                }
                if (tokens.ElementAt(i).lexema == ")")
                {
                    parentesis--;
                    hayParentesis = true;
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