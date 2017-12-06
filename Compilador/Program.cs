using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading.Tasks;

namespace Compilador
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Introduzca la direccion del archivo: ");
            String x = Console.ReadLine();
            Lexico lex = new Lexico(x);
            Sintactico sin = new Sintactico(lex);
            sin.imprimir();
            Console.ReadLine();
        }//Main
    }//class Program

    struct token
    {//tipos son identifier, keyword, operator, special, constant,punctuation, comparison, yeah
        public string tipo;
        public string lexema;
        public int linea;
        public int index;
        public token(string t, string l, int line, int i)
        {
            tipo = t;
            lexema = l;
            linea = line;
            index = i;
        }//token
         //tome strings con todo y colmilla
    }//token

    class Lexico
    {
        string[] keyword = { "entero", "palabra", "real", "caracter" };
        string[] metodo = { "para", "mientras", "si", "imprimir", "simprimir", "leer", "sino" };
        string[] oper = { "+", "-", "*", "/", "^", "%", "=" };
        string[] cmp = { ">", "<", ">=", "<=", "==", "!=" };
        string[] corch = { "{", "}", "(", ")", "[", "]" };
        string prog = "";
        int linea = 0;
        int index = 0;
        LinkedList<token> tokens = new LinkedList<token>();
        LinkedList<string> errors = new LinkedList<string>();

        public Lexico(string lex)
        {
            StreamReader obj = new StreamReader(lex);
            while (prog != null)
            {
                prog = obj.ReadLine();
                linea += 1;
                if (prog != null)
                {
                    index = 0;
                    control();
                }//if(prog!=null)
                else
                {
                    obj.Close();
                    //print();
                }//else
            }//while
             //give it the line of codeprog=lex;



            //que pare al ver un ; o ,  ... los reconoce como algun tipo de token?


        }//Lexico
        public string read()
        {//string must be empty first time it comes
            string wrd = "";
            //bool first=true;
            while (index < prog.Length)
            {
                if (prog.ElementAt(index) == 32 && wrd != string.Empty)
                {
                    return wrd;
                }//if space and i have something
                else
                {
                    if (prog.ElementAt(index) == 39 || prog.ElementAt(index) == 34)
                    {
                        if (wrd == string.Empty)
                        {
                            return expressRead();
                        }//if(prog==string.Empty)
                        else
                        {
                            return wrd;
                        }//else
                    }//if(prog.ElementAt(index)=="'"|| prog.ElementAt(index)==34)
                    if (oper.Contains(prog.ElementAt(index).ToString()) || corch.Contains(prog.ElementAt(index).ToString()) || prog.ElementAt(index) == 59 || prog.ElementAt(index) == 44 || prog.ElementAt(index) == 62 || prog.ElementAt(index) == 60 || prog.ElementAt(index) == 33)
                    {
                        if (wrd == string.Empty)
                        {
                            if (prog.ElementAt(index) >= 60 && prog.ElementAt(index) <= 62 || prog.ElementAt(index) == 33)
                            {
                                wrd = getCompare();
                                index += 1;
                                if (wrd.Length > 1) { index += 1; }//2 valores de comparacion
                            }//first item of a comparison
                            else
                            {
                                wrd += prog.ElementAt(index);
                                index += 1;
                            }//no parece ser comparacion

                            return wrd;
                        }//if(prog==string.Empty)
                        else
                        {
                            return wrd;
                        }//else
                    }//if(oper.Contains(prog.ElementAt(index)) || corch.Contains(prog.ElementAt(index))) 
                    if (prog.ElementAt(index) != 32 && prog.ElementAt(index) != 9 && prog.ElementAt(index) != 0)
                    { wrd += prog.ElementAt(index); }//aint blank space
                    index += 1;
                }//else
            }//while
            if (wrd != string.Empty)
            { return wrd; }
            else
            {
                return string.Empty;
            }//else

        }//read()
        public string expressRead()
        {
            bool cnt = true;
            string wrd = "";
            do
            {
                wrd += prog.ElementAt(index);
                if (prog.ElementAt(index) == 39 || prog.ElementAt(index) == 34)
                {
                    if (cnt) { cnt = false; }
                    else
                    {
                        index += 1;
                        return wrd;
                    }//not first time its grabbed
                }//if
                index += 1;
            } while (index < prog.Length);
            return wrd;//llego al fin de la linea || tomar en cuenta si halla un ;
        }//expressRead()

        public void control()
        {

            string wrd = "";
            while (index < prog.Length)
            {
                wrd = read();
                if (wrd != string.Empty)
                {
                    token x = new token("nell", wrd, linea, index);
                    if (keyword.Contains(wrd.ToLower()))
                    {
                        x = new token("keyword", wrd.ToLower(), linea, index);
                    }//if(keyword.Contains(wrd))
                    else
                    {
                        if (metodo.Contains(wrd.ToLower()))//esto lo metio el licea para saber si es una palabra reservada que obligatoriamente necesita parentesis despues
                        {
                            x = new token("metodo", wrd.ToLower(), linea, index);
                        }
                        else
                        {
                            if (oper.Contains(wrd))
                            {
                                x = new token("operator", wrd, linea, index);
                            }//oper.contains wrd
                            else
                            {
                                if (cmp.Contains(wrd))
                                {
                                    x = new token("comparison", wrd, linea, index);
                                }//if comparison
                                else
                                {
                                    if (corch.Contains(wrd))
                                    {
                                        x = new token("corchete", wrd, linea, index);
                                    }//if special has it 
                                    else
                                    {
                                        if (wrd.Length == 1 && wrd.ElementAt(0) == 44 || wrd.Length == 1 && wrd.ElementAt(0) == 59)
                                        {
                                            x = new token("punctuation", wrd, linea, index);
                                        }//if wrd(wrd==44 || wrd==59)
                                        else
                                        {
                                            double m = 0;
                                            if (double.TryParse(wrd, out m))
                                            {
                                                x = new token("constant", wrd, linea, index);
                                            }//if
                                            else
                                            {
                                                if (wrd.ElementAt(0) >= 65 && wrd.ElementAt(0) <= 90 && onlyLetters(wrd) || wrd.ElementAt(0) >= 97 && wrd.ElementAt(0) <= 122 && onlyLetters(wrd))
                                                {
                                                    x = new token("identifier", wrd, linea, index);
                                                }//if
                                                else
                                                {
                                                    if (wrd.ElementAt(0) == 34 || wrd.ElementAt(0) == 39)
                                                    {
                                                        x = new token("string", wrd, linea, index);
                                                    }//if(wrd.ElementAt(0)==34 || wrd.ElementAt(0)==39)
                                                    else
                                                    {
                                                        x = new token("nell", wrd, linea, index);
                                                        errors.AddLast("valor irreconocible:<" + wrd + "> en la linea: " + linea + "  posicion: " + index);
                                                    }//else
                                                }//else
                                            }//else 
                                        }//else
                                    }//else not a metodo
                                }//else
                            }//else not a comparison
                        }//else
                    }//else

                    if (x.tipo != "nell")
                    {
                        tokens.AddLast(x);
                    }//if(x.tipo!="nell")

                }//if(wrd!= string.Empty)

            }//while index is smaller

        }//LinkedLinst

        public LinkedList<string> getErrors()
        {
            return errors;
        }//getErrors
        public LinkedList<token> getTokens()
        {
            return tokens;
        }//getErrors
        public void print()
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                Console.WriteLine("<" + tokens.ElementAt(i).tipo + "," + tokens.ElementAt(i).lexema + ">");
            }//for(int i=0; i<tokens.Count; i++)
            if (errors.Count > 0)
            {
                Console.WriteLine("los errores son:");
            }
            for (int i = 0; i < errors.Count; i++)
            {
                Console.WriteLine(errors.ElementAt(i));
            }//for(int i=0; i<tokens.Count; i++)
        }//print
        public bool onlyLetters(string x, int c = 0)
        {
            if (c < x.Length)
            {
                if (x.ElementAt(c) >= 65 && x.ElementAt(c) <= 90 || x.ElementAt(c) >= 97 && x.ElementAt(c) <= 122 || x.ElementAt(c) >= 48 && x.ElementAt(c) <= 57)
                {
                    return onlyLetters(x, c + 1);
                }//it's a letter
                else
                {
                    return false;
                }//not a letter
            }//if(c<x.Length)
            else
            {
                return true;
            }//else
        }//onlyLetters
        public string getCompare()
        {
            string wrd = "" + prog.ElementAt(index);
            if (index + 1 < prog.Length && prog.ElementAt(index + 1) == 61)
            {
                return wrd += prog.ElementAt(index + 1);
            }//if(index+1< prog.Length&& prog.ElementAt(index+1)==61)
            else
            {
                return wrd;
            }//else
        }//getCompare()
    }//Lexico
}//class Compilador
