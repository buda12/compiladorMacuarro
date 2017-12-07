using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    enum TipoCaracter
    {
        Invalido,
        Operador,
        Operando,
        ParentesisA,
        ParentesisB,
        Constante
    }
    class Calculadora
    {
        decimal a, b, c;
        int digAfterPunto = 0;
        bool tienePunto = false;
        private string expresion;
        private string postfija;
        private bool hayError = false;
        public string procedimiento { get; set; }
        public string procedimientoBinario { get; set; }
        public string error { get; set; }
        public string PostFija
        {
            get { return postfija; }
        }
        public string InFija
        {
            get { return expresion; }
            set
            {
                expresion = value;
                postfija = ConvertirInfija(value);
            }
        }

        public decimal F()
        {
            return EvaluarPostfija(postfija);
        }

        public Calculadora(string expresion, decimal a, decimal b, decimal c)
        {
            this.expresion = expresion;
            this.a = a;
            this.b = b;
            this.c = c;
            this.postfija = ConvertirInfija(expresion);
        }

        public string convertirANotacionCientifica(decimal number)
        {
            string notCient = string.Empty;
            string aux = string.Empty;
            string token = string.Empty;
            int exp = 0;
            int cont = 0;
            bool esNegativo = false;

            if (number < 0)
            {
                esNegativo = true;
                number *= -1;
            }
            if (number >= 10)
            {
                while (number >= 10)
                {
                    number /= 10;
                    exp++;
                }
            }
            else if (number <= 1)
            {
                while (number <= 1)
                {
                    number *= 10;
                    exp--;
                }
            }

            aux = number.ToString();

            if (esNegativo) { notCient = "-"; }

            for (int i = 0; i < number.ToString().Length; i++)
            {
                token = aux[i].ToString();
                if (token != ".")
                    cont++;
                else if (number - Convert.ToInt64(number) == 0)
                {
                    break;
                }
                notCient += token;
                if (cont >= 8) { break; }
            }

            notCient += "E" + exp;
            // exponente = exp;
            return notCient;
        }

        public string expresionConNotacionCientifica(string expresion)
        {
            string aux = string.Empty;
            string numero = "";
            string token = string.Empty;
            int numDig = 0;
            for (int i = 0; i < expresion.Length; i++)
            {
                token = expresion[i].ToString();
                if (Char.IsDigit(token[0]) || token == ".")
                {
                    numero += token;
                }
                if (token == " ")
                {
                    if (numero != "")
                    {
                        if (contarDigitos(decimal.Parse(numero)) >= 8)
                        {
                            numDig = contarDigitos(decimal.Parse(numero));
                            aux = convertirANotacionCientifica(decimal.Parse(numero));
                            numero = aux;
                            aux = expresion.Substring(0, i - numDig);
                            aux += numero + expresion.Substring(i);
                            expresion = aux;
                        }
                    }
                    numero = "";
                }
            }
            error += Environment.NewLine + expresion;
            return expresion;
        }

        public decimal EvaluarPostfija(string expresion)
        {
            if (hayError) return 0;

            Stack<decimal> pila = new Stack<decimal>();
            string[] tokens = expresion.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            decimal operando = 0;

            foreach (string token in tokens)
            {
                TipoCaracter tipo = GetTipo(token, 0);
                //expresion = expresionConNotacionCientifica(expresion);

                if (tipo == TipoCaracter.Operador)
                    EjecutarOperacion(pila, token);
                else if (decimal.TryParse(token, out operando))
                    pila.Push(operando);
                else if (token == "A")
                    pila.Push(a);
                else if (token == "B")
                    pila.Push(b);
                else if (token == "C")
                    pila.Push(c);
                else { error = ("Error formato numerico no valido"); hayError = true; }
            }

            if (pila.Count != 1)
            {
                error = ("No se ha podido evaluar la expresion");
                hayError = true;
            }
            return pila.Pop();
        }

        public int contarDigitos(decimal number)
        {
            string token = string.Empty;
            int numDig = 0;
            digAfterPunto = 0;
            string aux = number.ToString();
            tienePunto = false;

            for (int i = 0; i < number.ToString().Length; i++)
            {
                token = aux[i].ToString();
                if (token != ".")
                {
                    numDig++;
                }
                else
                {
                    tienePunto = true;
                    digAfterPunto = i;
                }
            }

            return numDig;
        }

        private void EjecutarOperacion(Stack<decimal> pila, string operacion)
        {
            decimal first = GetOperando(pila);

            switch (operacion)
            {
                case "+":
                    procedimiento += pila.First().ToString() + "+" + first.ToString() + System.Environment.NewLine;
                    pila.Push(Math.Round(GetOperando(pila) + first, 7));
                    break;
                case "-":
                    procedimiento += pila.First().ToString() + "-" + first.ToString() + System.Environment.NewLine;
                    pila.Push(Math.Round(GetOperando(pila) - first, 7));
                    break;
                case "*":
                    procedimiento += pila.First().ToString() + "*" + first.ToString() + System.Environment.NewLine;
                    pila.Push(Math.Round(GetOperando(pila) * first, 7));
                    break;
                case "/":
                    if (first == 0) { error = ("Error" + System.Environment.NewLine + "Division por cero"); hayError = true; }
                    else
                    {
                        procedimiento += pila.First().ToString() + "/" + first.ToString() + System.Environment.NewLine;
                        pila.Push(Math.Round(GetOperando(pila) / first, 7));
                    }
                    break;
                case "^":
                    if (pila.First() < 0 && first < 1) { error = ("Error" + System.Environment.NewLine + "No se puede sacar raiz de numeros negativos"); hayError = true; }
                    else
                    {
                        procedimiento += pila.First().ToString() + "^" + first.ToString() + System.Environment.NewLine;
                        pila.Push(Math.Round(Convert.ToDecimal(Math.Pow(Convert.ToDouble(GetOperando(pila)), Convert.ToDouble(first))), 7));
                    }
                    break;
                case "E":
                    if (first - Convert.ToInt64(first) != 0) { error = ("Error" + System.Environment.NewLine + "Las potencias tienen que ser numeros enteros"); hayError = true; }
                    else
                    {
                        procedimiento += pila.First().ToString() + "*10^" + first.ToString() + Environment.NewLine;
                        pila.Push(Math.Round(GetOperando(pila) * Convert.ToDecimal(Math.Pow(10, Convert.ToDouble(first))), 7));
                    }
                    break;
                default:
                    error = ("Error operacion no admitida");
                    hayError = true;
                    break;
            }
        }

        private decimal GetOperando(Stack<decimal> pila)
        {
            if (pila.Count == 0) error = ("Error faltan operandos");
            else
            {
                return pila.Pop();
            }
            return 0;
        }

        private TipoCaracter GetTipo(string token, int posicion)
        {
            if (char.IsDigit(token[0])) return TipoCaracter.Operando;

            switch (token)
            {
                case "-1":
                case ".":
                    return TipoCaracter.Operando;
                case "A":
                case "B":
                case "C":
                    return TipoCaracter.Constante;
                case "/":
                case "*":
                case "-":
                case "+":
                case "^":
                case "E":
                    return TipoCaracter.Operador;
                case "(":
                    return TipoCaracter.ParentesisA;
                case ")":
                    return TipoCaracter.ParentesisB;
                default:
                    error = ("Error en la posicion " + posicion + System.Environment.NewLine + "Operador invalido");
                    hayError = true;
                    return TipoCaracter.Invalido;
            }
        }

        public string ConvertirInfija(string expresion)
        {
            StringBuilder salida = new StringBuilder();
            Stack<string> operadores = new Stack<string>();
            string token = string.Empty;
            TipoCaracter tipo = TipoCaracter.Invalido;
            TipoCaracter last = TipoCaracter.Invalido;
            int contOperando = 0;
            bool faltaOperando = false;
            int posicionUltimoOperador = 0;
            int operacion = 0;
            int digitos = 0;

            for (int i = 0; i < expresion.Length; i++)
            {
                token = expresion[i].ToString();

                if (string.IsNullOrWhiteSpace(token)) continue;

                tipo = GetTipo(token, i);

                if (tipo == TipoCaracter.Operando)
                {
                    if (i > 0)
                    {
                        if (expresion[i - 1] == '.' && token == ".")
                        {
                            error = ("Error en la posicion " + i + Environment.NewLine + "No se puede poner dos puntos seguidos"); hayError = true;
                        }
                    }

                    //if (digitos >= 8) { }
                    //else
                    //{

                    if (last == TipoCaracter.Constante || last == TipoCaracter.ParentesisB)
                        ApilarOperador(salida, operadores, "*");

                    salida.Append(token);
                    contOperando++;
                    faltaOperando = false;
                    if (token != ".") { digitos++; }
                    //}
                }
                else if (tipo == TipoCaracter.Constante)
                {
                    VerificarMultiplicacionOculta(salida, operadores, last);
                    salida.Append(" " + token + " ");
                    contOperando++;
                    faltaOperando = false;
                    digitos = 0;
                }
                else if (tipo == TipoCaracter.Operador)
                {
                    if (contOperando == 0 && Prioridad(token) != 1) { error = ("Error en la posicion " + i + Environment.NewLine + "No se puede comenzar la expresion con este operador"); hayError = true; }
                    if (contOperando == 0 && token == "+") { continue; }
                    if (last == TipoCaracter.Operador && token != "-")
                    {
                        error = ("Error despues de la posicion " + i + Environment.NewLine + "Falta operando");
                        hayError = true;
                    }

                    if ((last == TipoCaracter.Operador && token == "-") ||
                        (last == TipoCaracter.Invalido && token == "-") ||
                        (last == TipoCaracter.ParentesisA && token == "-"))
                    {
                        salida.Append(" -1 ");
                        operadores.Push("*");
                    }
                    else
                    {
                        ApilarOperador(salida, operadores, token);
                        faltaOperando = true;
                        posicionUltimoOperador = i;
                        operacion++;
                    }
                    digitos = 0;
                }
                else if (tipo == TipoCaracter.ParentesisA)
                {
                    VerificarMultiplicacionOculta(salida, operadores, last);
                    operadores.Push("(");
                    digitos = 0;
                }
                else if (tipo == TipoCaracter.ParentesisB)
                {
                    DesapilarParentesis(salida, operadores);
                    digitos = 0;
                }
                else
                {
                    int index = expresion.IndexOf('(', i);
                    if (index < 0)
                    {
                        error = ("Error en sintaxis");
                        hayError = true;
                    }
                    else
                    {
                        string newtoken = expresion.Substring(i, index - i);
                        i = index - 1;

                        tipo = GetTipo(newtoken, i);
                    }
                    digitos = 0;
                }
                last = tipo;
            }

            if (faltaOperando)
            {
                error = ("Error despues de la posicion " + posicionUltimoOperador + Environment.NewLine + "Falta operando");
                hayError = true;
            }

            if (operacion > 8) { error = ("Error" + Environment.NewLine + "Sobrepasa el limite de 8 operaciones"); hayError = true; }

            VaciarOperandos(salida, operadores);
            RemoverEspaciosVacios(salida);

            return salida.ToString();
        }

        private void DesapilarParentesis(StringBuilder salida, Stack<string> operadores)
        {
            if (operadores.Count == 0)
            {
                error = ("Error falta parentesis de apertura (");
                hayError = true;
                return;
            }
            string sop = operadores.Pop();
            salida.Append(" ");

            while (sop != "(")
            {
                salida.Append(sop + " ");

                if (operadores.Count == 0)
                {
                    error = ("Error falta parentesis de apertura (");
                    hayError = true;
                    return;
                }

                sop = operadores.Pop();
            }
        }

        private void VerificarMultiplicacionOculta(StringBuilder salida, Stack<string> operadores, TipoCaracter last)
        {
            if (last == TipoCaracter.Constante || last == TipoCaracter.Operando || last == TipoCaracter.ParentesisB)
                ApilarOperador(salida, operadores, "*");
        }

        private void RemoverEspaciosVacios(StringBuilder salida)
        {
            if (salida[0] == ' ') salida.Remove(0, 1);
            if (salida[salida.Length - 1] == ' ') salida.Remove(salida.Length - 1, 1);

            for (int i = 0; i < salida.Length - 1; i++)
            {
                if (salida[i] == ' ' && salida[i + 1] == ' ')
                    salida.Remove(i, 1);
            }
        }

        private void VaciarOperandos(StringBuilder salida, Stack<string> operadores)
        {
            salida.Append(" ");

            while (operadores.Count > 0)
            {
                string sop = operadores.Pop();

                if (sop == "(")
                {
                    error = ("Error falta parentesis de cierre )");
                    hayError = true;
                }

                salida.Append(sop + " ");
            }
        }

        private void ApilarOperador(StringBuilder salida, Stack<string> operadores, string token)
        {
            salida.Append(" ");
            if (operadores.Count > 0) DesapilarOperando(token, operadores, salida);
            operadores.Push(token);
        }

        private void DesapilarOperando(string operador, Stack<string> operadores, StringBuilder salida)
        {
            int pc = Prioridad(operador);
            string op = operadores.Pop();
            int po = Prioridad(op);

            while (pc <= po)
            {
                salida.Append(op + " ");

                if (operadores.Count == 0) break;

                op = operadores.Pop();
                po = Prioridad(op);
            }

            if (pc > po)
                operadores.Push(op);
        }

        private byte Prioridad(string operando)
        {
            switch (operando)
            {
                case "(":
                    return 0;
                case "+":
                case "-":
                    return 1;
                case "*":
                case "/":
                case "E":
                    return 2;
                case "^":
                    return 3;
                default:
                    return 4;
            }
        }
    }
}
