using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;
using Unity.VisualScripting;


public class Memory
{
    public List<Dictionary<string, object>> memory_stack;
    public Memory()
    {
        memory_stack = new List<Dictionary<string, object>>();
    }
    public void Pop() => memory_stack.RemoveAt(memory_stack.Count - 1);
    public Dictionary<string, object> Peek() => memory_stack.ElementAt(memory_stack.Count - 1);
}

public class Interpreter
{
    public Dictionary<string, object> current_stack;
    public Memory Memory;
    public Parser parser;
    public GameKey GameKey;
    public JuegoGWENT juego;
    public AST tree;

    public Interpreter(AST tree, Parser parser, GameKey gameKey, JuegoGWENT juego)
    {
        this.parser = parser;
        Memory = new Memory();
        Memory.memory_stack.Add((new Dictionary<string, object>()));
        current_stack = Memory.Peek();
        this.GameKey = gameKey;
        this.juego = juego;
        this.tree = tree;
    }
    public void To_interpret() => Visit_Block((Block)tree);
    public void Visit_Block(Block node)
    {
        foreach (AST statement in node.SList)
            Visit_statement(statement);
    }
    public void Visit_statement(AST node)
    { //nod
        if (node is RandomPower rPower) VisitRandomPower(rPower);
        else if (node is SelectPower select) VisitSelectPower(select);
        else if (node is PosPower posPower) VisitPosPower(posPower);
        else if (node is Destroy) VisitDestroy();
        else if (node is SelfPower pow) Visit_PowSelf(pow);
        else if (node is TargetedPower power) VisitTargetedPower(power);
        else if (node is OriginalPower) VisitOriginalPower();
        else if (node is OriginalSelectPower oSPower) VisitOriginalSelectPower(oSPower);
       
        else if (node is Assign A_statement) Visit_Assign_statement(A_statement);
        else if (node is While @while) Visit_while(@while);
        else if (node is If IF) Visit_if(IF);
        else if (node is MethodCall method) Visit_void_Method_Call(method);
        else if (node is VariableD or MethodD or Empty) return;
    }



    //Declaraciones power
    private void VisitOriginalSelectPower(OriginalSelectPower node)
    {   //devuelve el poder de las cartas a su valor original
        Func<Card, bool> select;
        switch (node.Stat)
        {
            case "empowered":
                select = IsPotencied;
                break;
            case "injured":
                select = IsInjured;
                break;
            default:
                select = AllCards;
                break;
        }
        List<Card> cards = GetCards(select, node.Field);
        foreach (Card card in cards)
            GameKey.actualPower[card] = card.Power;
    }
    private void VisitRandomPower(RandomPower node)
    {
        if(EmptyField(node.Field)) return;
        for (int i = 0; i < Visit_Expr(node.Targets); i++)
            Debug.Log("");
            GameKey.actualPower[GetRandomCard(node.Field)] += Visit_Expr(node.Change);
    }
    private void Visit_PowSelf(SelfPower node)
    {
        GameKey.actualPower[juego.ActualMove.Card] += Visit_Expr(node.Change);
    }
    private void VisitDestroy()
    {
        var pos1 = juego.ActualMove.Target.Item1;
        var pos2 = juego.ActualMove.Target.Item2;
        switch (juego.ActualMove.Card.Efecttype)
        {
            case EfectType.TargetAllie:
                if (juego.Field[juego.PlayerInTurn][pos1, pos2] != null)
                    GameKey.actualPower[juego.Field[juego.PlayerInTurn][pos1, pos2]] = 0;
                break;
            case EfectType.TargetEnemy:
                if (juego.Field[juego.PlayerWaiting][pos1, pos2] != null)
                    GameKey.actualPower[juego.Field[juego.PlayerWaiting][pos1, pos2]] = 0;
                break;
            default: return;
        }
    }
    public void VisitTargetedPower(TargetedPower node)
    {
        int pos1 = juego.ActualMove.Target.Item1;
        int pos2 = juego.ActualMove.Target.Item2;
        switch (juego.ActualMove.Card.Efecttype)
        {
            case EfectType.TargetAllie:
                if (juego.Field[juego.PlayerInTurn][pos1, pos2] != null)
                    GameKey.actualPower[juego.Field[juego.PlayerInTurn][pos1, pos2]] += Visit_Expr(node.Chage);
                break;
            case EfectType.TargetEnemy:
                if (juego.Field[juego.PlayerWaiting][pos1, pos2] != null)
                    GameKey.actualPower[juego.Field[juego.PlayerWaiting][pos1, pos2]] += Visit_Expr(node.Chage);
                break;
            default: return;
        }
    }
    private void VisitOriginalPower()
    {
        int pos1 = juego.ActualMove.Target.Item1;
        int pos2 = juego.ActualMove.Target.Item2;
        switch (juego.ActualMove.Card.Efecttype)
        {
            case EfectType.TargetAllie:
                if (juego.Field[juego.PlayerInTurn][pos1, pos2] != null)
                    GameKey.actualPower[juego.Field[juego.PlayerInTurn][pos1, pos2]] =
                        juego.Field[juego.PlayerInTurn][pos1, pos2].Power;
                break;
            case EfectType.TargetEnemy:
                if (juego.Field[juego.PlayerWaiting][pos1, pos2] != null)
                    GameKey.actualPower[juego.Field[juego.PlayerWaiting][pos1, pos2]] =
                        juego.Field[juego.PlayerWaiting][pos1, pos2].Power;
                break;
            default: return;
        }
    }
    private void VisitPosPower(PosPower posPower)
    {//altera el poder de las cartas adyacentes
        throw new NotImplementedException();
    }
    private void VisitSelectPower(SelectPower node)
    {
        List<Card> cards = new List<Card>();
        switch (node.Stat)
        {
            case "empowered":
                cards = GetCards(IsPotencied, node.Field);
                break;
            case "injured":
                cards = GetCards(IsInjured, node.Field);
                break;
            case "lessP":
                cards.Add(GetCards(Mayor, node.Field));
                break;
            case "mostP":
                cards.Add(GetCards(Menor, node.Field));
                break;
            case "all":
                cards = GetCards(AllCards, node.Field);
                break;
            default: throw new Exception("Sin stat");
        }
        foreach (Card card in cards)
            GameKey.actualPower[card] += Visit_Expr(node.Change);
    }

    //Declaraciones del lenguaje
    private void Visit_Assign_statement(Assign node)
    {
        string ID = node.Var.Id;
        if (node.Type == "bool")
        {
            bool value = Visit_Bexpr(node.Expr);

            if (current_stack.ContainsKey(ID))
                current_stack[ID] = value;

            else current_stack.Add(ID, value);
            return;
        }
        if (node.Type == "int")
        {
            int value = Visit_Expr(node.Expr);
            if (current_stack.ContainsKey(ID))
                current_stack[ID] = value;

            else current_stack.Add(ID, value);
            return;
        }
        if (node.Type == "string")
        {
            string value = Visit_string((String)node.Expr);
            if (current_stack.ContainsKey(ID))
                current_stack[ID] = value;

            else current_stack.Add(ID, value);
        }
    }
    public void Visit_if(If node)
    {
        if (Visit_Bexpr(node.Condition))
            Visit_Block(node.Block);
    }
    private void Visit_while(While node)
    {
        while (Visit_Bexpr(node.Condition))
            Visit_Block(node.Block);
    }
    private void Assign_Parameters(MethodCall node)
    {
        List<(string, string)> formal_parameters = node.MethodReference.formal_parameters.parameters;
        List<(string, object)> arguments = new();

        for (int i = 0; i < formal_parameters.Count; i++)
        {
            string ID = formal_parameters.ElementAt(i).Item2;
            object value;
            switch (formal_parameters.ElementAt(i).Item1)
            {
                case "int":
                    value = Visit_Expr(node.ActualParameters[i]);
                    break;
                case "bool":
                    value = Visit_Bexpr(node.ActualParameters[i]);
                    break;
                default:
                    value = Visit_string((String)node.ActualParameters[i]);
                    break;
            }
            arguments.Add((ID, value));
        }

        Memory.memory_stack.Add((new Dictionary<string, object>()));
        current_stack = Memory.Peek();

        foreach ((string, object) item in arguments)
            current_stack.Add(item.Item1, item.Item2);

    }
    private void Visit_void_Method_Call(MethodCall node)
    {
        //CW
        if (node.Id == "print")
        {
            if (node.ActualParameters.ElementAt(0) is Var var)
                Console.WriteLine(current_stack[var.Id]);
            else if (node.ActualParameters.ElementAt(0) is String str)
                Console.WriteLine(str.Value);
            return;
        }
        // asignando los parametros reales a los parametros formales
        Assign_Parameters(node);
        Visit_Block(node.MethodReference.block);

        Memory.Pop();
        current_stack = Memory.Peek();
    }

    //Declaracion para obtener datos del juego
    private int VisitGet(Get get)
    {
        Card card;
        switch (get.Stat)
        {
            case "lessP":
                card = GetCards(Menor, get.Field);
                if(card != null) return GameKey.actualPower[card];
                return 0;
            case "mostP":
                card = GetCards(Menor, get.Field);
                if (card != null) return GameKey.actualPower[GetCards(Mayor, get.Field)];
                return 0;
            case "injured":
                return GetCards(IsInjured, get.Field).Count;
            case "empowered":
                return GetCards(IsPotencied, get.Field).Count;
            default:
                return 0;
        }
    }

    //Seccion de expresiones aritmeticas
    public int Visit_Expr(AST node)
    {
        switch (node)
        {
            // Identifies wich AST type is
            case Integer integer:
                return Visit_integer(integer);
            case Var var:
                return Visit_var(var);
            case Unitary_op unitaryOp:
                return Visit_Uni_op(unitaryOp);
            case Binary_op binary:
                return Visit_Bin_op(binary);
            case MethodCall method:
                return Visit_intMethod(method);
            case Get get:
                return VisitGet(get);
            default:
                throw new Exception("Wrong expression");
        }
    }
    public int Visit_intMethod(MethodCall node)
    {
        Assign_Parameters(node);
        int value = Visit_intBlock(node.MethodReference.block);

        Memory.Pop();
        current_stack = Memory.Peek();
        return value;
    }
    public int Visit_intBlock(Block block)
    {
        foreach (AST statement in block.SList)
        {
            if (statement is Return re) return Visit_Expr(re.Retorno);
            Visit_statement(statement);
        }
        throw new Exception("Sin retorno");
    }
    public int Visit_integer(Integer node) => node.Value;
    public int Visit_var(Var node) => (int)current_stack[node.Id];
    public int Visit_Uni_op(Unitary_op node) => -1 * Visit_Expr(node.Right);
    public int Visit_Bin_op(Binary_op node)
    {
        //left op rigth
        if (node.Op.type == "Plus") return Visit_Expr(node.Left) + Visit_Expr(node.Right);
        if (node.Op.type == "Minus") return Visit_Expr(node.Left) - Visit_Expr(node.Right);
        if (node.Op.type == "Mult") return Visit_Expr(node.Left) * Visit_Expr(node.Right);
        if (node.Op.type == "Div") return Visit_Expr(node.Left) / Visit_Expr(node.Right);

        throw new Exception("Invalid Sintax");
    }
    
    //Seccion de expresiones boleanas
    public bool Visit_Bblock(Block block)
    {
        foreach (AST statement in block.SList)
        {
            if (statement is Return re) return Visit_Bexpr(re.Retorno);
            Visit_statement(statement);
        }
        throw new Exception("Sin retorno");
    }
    public bool Visit_Bmethod(MethodCall node)
    {
        Assign_Parameters(node);
        bool value = Visit_Bblock(node.MethodReference.block);

        Memory.Pop();
        current_stack = Memory.Peek();
        return value;
    }
    public bool Visit_Bexpr(AST node)
    {
        if (node is Binary_op binary_op) return Visit_Bexpr(binary_op);
        if (node is Bool boolean) return Visit_Bool(boolean);
        if (node is Var var) return Visit_Bvar(var);
        if (node is MethodCall method) return Visit_Bmethod(method);
        else throw new Exception("Wrong boolean expression");
    }
    public bool Visit_Bexpr(Binary_op node)
    {
        switch ((string)node.Op.value)
        {
            case ">": return Visit_Expr(node.Left) > Visit_Expr(node.Right);
            case "<": return Visit_Expr(node.Left) < Visit_Expr(node.Right);
            case ">=": return Visit_Expr(node.Left) >= Visit_Expr(node.Right);
            case "<=": return Visit_Expr(node.Left) <= Visit_Expr(node.Right);
            case "==": return Visit_Expr(node.Left) == Visit_Expr(node.Right);
            case "!=": return Visit_Expr(node.Left) != Visit_Expr(node.Right);
            case "or": return Visit_Bexpr(node.Left) || Visit_Bexpr(node.Right);
            case "and": return Visit_Bexpr(node.Left) && Visit_Bexpr(node.Right);

            default: throw new Exception("Comparador inexistente en el lenguaje");
        }
    }
    public bool Visit_Bool(Bool node) => node.Value;
    public bool Visit_Bvar(Var var) => (bool)current_stack[var.Id];
    
    //Metodo para visitar expresion de tipo string
    public string Visit_string(String node) => node.Value;
    
    //Metodos de recorrido del campo
    public Card GetRandomCard(Card[,] field)
    {
        bool finded = false;
        foreach (var card in field)
        {
            if (card != null)
            {
                finded = true;
                break;
            }
        }
        if (!finded) return null;
        else
        {
            (int, int) position = (new System.Random().Next(0, 2), new System.Random().Next(0, 5));
            while (field[position.Item1, position.Item2] == null)
            {
                position = (new System.Random().Next(0, 2), new System.Random().Next(0, 5));
            }
            return field[position.Item1, position.Item2];
        }
    }
    public List<Card> GetCards(Func<Card, bool> function, Card[,] field)
    {
        List<Card> cards = new();
        foreach (var card in field)
        {
            if (card != null)
                if (function(card)) cards.Add(card);
        }
        return cards;
    }
    public Card GetCards(Func<Card, Card, Card> function, Card[,] place)
    {
        Card result = null;
        foreach (var card in place)
        {
            if (card != null)
            {
                result = card;
                break;
            }
        }
        if (result == null) return null;
        foreach (Card card in place)
        {
            if (card != null)
                result = function(result, card);
        }
        return result;
    }
    
    //Functions
    Card Mayor(Card x, Card y) => x.Power > y.Power ? x : y;
    Card Menor(Card x, Card y) => x.Power < y.Power ? x : y;
    bool IsPotencied(Card x) => juego.ActualPower[x] > x.Power ? true : false;
    bool AllCards(Card x) => true;
    bool IsInjured(Card x) => juego.ActualPower[x] < x.Power ? true : false;
    private bool EmptyField(Card[,] field)
    {
        foreach (var card in field)
        {
            if (card != null) return false;
        }
        return true;
    }

}