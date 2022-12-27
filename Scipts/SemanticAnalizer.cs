using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;




public class Symbol
{
    public string ID;
    public Symbol(string ID)
    {
        this.ID = ID;
    }
}

public class Type_symbol : Symbol
{
    public string type;
    public Type_symbol(string ID) : base(ID)
    {
        this.type = ID;
    }
}
public class Var_symbol : Symbol
{
    public string type;
    public Var_symbol(string ID, string type) : base(ID)
    {
        this.type = type;
    }
}

public class Method_symbol : Symbol
{
    public string type;
    public Parameters formal_parameters;
    public Block block;

    public Method_symbol(string type, string ID, Parameters parameters, Block block) : base(ID)
    {
        this.type = type;
        this.formal_parameters = parameters;
        this.block = block;
    }
}
public class Build_out : Symbol
{
    public Build_out(string ID) : base(ID) { }
}
public class Function_Symbol : Symbol
{
    //Functions does return a value
    public Token Return_type;
    public List<object> parameters;

    public Function_Symbol(string ID, Token Return_type, List<object> parameters) : base(ID)
    {
        this.Return_type = Return_type;
        this.parameters = parameters;
    }
}
public class Scope
{
    public Dictionary<string, Symbol> Symbol_tab;
    public Scope father;

    public Scope(Scope father, Dictionary<string, Symbol> parameters)
    {
        this.father = father;
        Symbol_tab = parameters;

        Symbol_tab.Add("int", new Type_symbol("int"));
        Symbol_tab.Add("string", new Type_symbol("string"));
        Symbol_tab.Add("bool", new Type_symbol("bool"));
        Symbol_tab.Add("print", new Build_out("print"));
    }
}

public class Semantic_analizer
{
    public GameKey gameKey;
    public JuegoGWENT Juego;
    public Scope Current_scope;
    public Parser parser;
    readonly AST Tree;

    public Semantic_analizer(AST tree, Parser parser, GameKey GameKey, JuegoGWENT Juego)
    {
        Current_scope = new Scope(null, new Dictionary<string, Symbol>());
        this.parser = parser;
        this.gameKey = GameKey;
        this.Juego = Juego;
        this.Tree = tree;
    }
    public void Check() => VisitVoidBlock((Block)Tree);
    public void VisitVoidBlock(Block node)
    {
        foreach (AST item in node.SList) Visit_statement(item);
    }
    public void Visit_return_Block(Block block, string type, string ID)
    {
        bool doesReturn = false;
        foreach (AST statement in block.SList)
        {
            if (statement is Return re)
            {
                if (type == "int") Visit_Expr(re.Retorno);
                if (type == "bool") Visit_Bexpr(re.Retorno);
                if (type == "string") Visit_string(re.Retorno);
                doesReturn = true;
            }
            else Visit_statement(statement);
        }
        if (!doesReturn) throw new Exception("The method " + ID + " does not have a return value");
    }

    public void Visit_statement(AST node)
    {
        //declaraciones de cartas
        if (node is TargetedPower targetedP)
            Visit_Expr(targetedP.Chage);
        else if (node is RandomPower randomPower)
            VisitRandomPower(randomPower);
        else if (node is SelfPower pow)
            Visit_Expr(pow.Change);
        else if (node is PosPower posPower)
            VisitPosPower(posPower);
        else if (node is SelectPower selectPower)
            VisitSelectPower(selectPower);
        else if (node is Destroy or OriginalPower)
            return;
        else if (node is OriginalSelectPower originalPower)
            VisitOriginalSelectPower(originalPower);
        //delcaraciones del lenguaje
        else if (node is While nodewhile)
            Visit_while(nodewhile);
        else if (node is If nodeif)
            Visit_If(nodeif);
        else if (node is VariableD variableD)
            Visit_Var_D(variableD);
        else if (node is MethodD method_D)
            Visit_Method_D(method_D);
        else if (node is MethodCall method_Call)
            Visit_Method_Call(method_Call, "void");
        else if (node is Assign assign_Statement) Visit_Asign_statement(assign_Statement);

        else if (node is Empty);
        else throw new Exception("Declaracion invalida");
    }

    private void VisitGet(Get get)
    {
        get.Field = get.Place switch
        {
            
            "field" => gameKey.field[Juego.PlayerInTurn],
            "eField" => gameKey.field[Juego.PlayerWaiting],
            _ => throw new Exception("Lugar no permitido"),
        };
        switch (get.Stat)
        {
            case "lessP":
                break;
            case "mostP":
                break;
            case "injured":
                break;
            case "empowered":
                break;
            case "all":
                break;
            default: throw new Exception("Estadistica de cartas no permitida");
        }
    }

    private void VisitSelectPower(SelectPower selectPower)
    {
        switch (selectPower.Place)
        {
            case "field":
                selectPower.Field = gameKey.field[Juego.PlayerInTurn];
                break;
            case "eField":
                selectPower.Field = gameKey.field[Juego.PlayerWaiting];
                break;
            default: throw new Exception("Lugar no permitido");
        }

        switch (selectPower.Stat)
        {
            case "lessP":
                break;
            case "mostP":
                break;
            case "all":
                break;
            default: throw new Exception("Estadistica de cartas no permitida");
        }
        Visit_Expr(selectPower.Change);
    }

    private void VisitPosPower(PosPower posPower)
    {
        throw new NotImplementedException();
    }

    private void VisitOriginalSelectPower(OriginalSelectPower node)
    {
        switch (node.Place)
        {
            case "field":
                node.Field = gameKey.field[Juego.PlayerInTurn];
                break;
            case "eField":
                node.Field = gameKey.field[Juego.PlayerWaiting];
                break;
            default: throw new Exception("Lugar no permitido");
        }
        switch (node.Stat)
        {
            case "lessP":
                break;
            case "mostP":
                break;
            default: throw new Exception("Estadistica de cartas no permitida");
        }
    }

    private void VisitRandomPower(RandomPower node)
    {
        switch (node.Place)
        {
            case "field":
                node.Field = Juego.Field[Juego.PlayerInTurn];
                break;
            case "eField":
                node.Field = Juego.Field[Juego.PlayerInTurn];
                break;
            default: throw new Exception("Lugar no permitido");
        }
        Visit_Expr(node.Change);
        Visit_Expr(node.Targets);
    }
   
    
    public void Visit_while(While node)
    {
        Visit_Bexpr(node.Condition);
        VisitVoidBlock(node.Block);
    }
    public void Visit_If(If node)
    {
        Visit_Bexpr(node.Condition);
        VisitVoidBlock(node.Block);
    }
    public void Visit_Method_D(MethodD node)
    {
        //Type - ID
        //Añadir variables declaradas en los parametros
        Current_scope = new Scope(Current_scope, new Dictionary<string, Symbol>());

        Visit_Parameters(node.Parameters);

        if (node.Type != "void") Visit_return_Block(node.Block, node.Type, node.Id);
        else VisitVoidBlock(node.Block);

        // Despues de visitar el metodo, vuelve al scope anterior y añade el metodo al symbol tab
        Current_scope = Current_scope.father;

        Current_scope.Symbol_tab.Add(node.Id, new Method_symbol(node.Type, node.Id, node.Parameters, node.Block));
    }
    public void Visit_Parameters(Parameters node)
    {
        //Type-ID
        foreach ((string, string) var in node.parameters)
        {
            Current_scope.Symbol_tab.Add(var.Item2, new Var_symbol(var.Item2, var.Item1));
        }
    }
    public void Visit_Asign_statement(Assign node)
    {
        if (Current_scope.Symbol_tab.ContainsKey(node.Var.Id))
        {
            Var_symbol var = (Var_symbol)Current_scope.Symbol_tab[node.Var.Id];

            if (var.type == "bool")
            {
                node.Type = "bool";
                Visit_Bexpr(node.Expr);
            }
            if (var.type == "int")
            {
                node.Type = "int";
                Visit_Expr(node.Expr);
            }
            if (var.type == "string")
            {
                node.Type = "string";
                Visit_string(node.Expr);
            }
            else throw new Exception("Variable de tipo inexistente");
        }
        else throw new Exception("Variable no delcarada");
    }
    public void Visit_Var_D(VariableD node)
    {
        // checks type and addS new vaR Symbol
        if (!Current_scope.Symbol_tab.ContainsKey(node.Type))
            throw new Exception("Tipo no existente");

        foreach (Var var in node.IDs)
        {
            if (!Current_scope.Symbol_tab.ContainsKey(var.Id))
                Current_scope.Symbol_tab.Add(var.Id, new Var_symbol(var.Id, node.Type));

            else throw new Exception("Declaracion de una variable ya existente");
        }
    }
    public void Visit_Method_Call(MethodCall node, string returnType)
    {
        //check if name is in table
        //chek number and types of actual parameters and formal parameters
        if (!Current_scope.Symbol_tab.ContainsKey(node.Id))
            throw new Exception("The method " + node.Id + " does not exist in current context");

        Method_symbol mref = (Method_symbol)Current_scope.Symbol_tab[node.Id];
        if(mref.type != returnType) throw new Exception("El metodo no devuelve "+ returnType);
        //check arguments

        int N = node.ActualParameters.Count;
        if (N != mref.formal_parameters.parameters.Count)
            throw new Exception("Cantidad de parametros incorrecta");

        for (int i = 0; i < N; i++)
        {
            switch (mref.formal_parameters.parameters.ElementAt(i).Item1)
            {
                case "int":
                    Visit_Expr(node.ActualParameters.ElementAt(i));
                    break;
                case "bool":
                    Visit_Bexpr(node.ActualParameters.ElementAt(i));
                    break;
                case "string":
                    Visit_string(node.ActualParameters.ElementAt(i));
                    break;
                default:
                    throw new Exception("Tipo no permitido");
            }
        }
        node.MethodReference = mref;
    }
    public void Visit_Var(Var node, string type)
    {
        if (!Current_scope.Symbol_tab.ContainsKey(node.Id))
            throw new Exception("Variable no declarada");

        Var_symbol var = (Var_symbol)Current_scope.Symbol_tab[node.Id];
        // ID y typo intercambiados
        if (var.type != type) throw new Exception("Variable y tipo no coincidentes");

        foreach (Token keyword in Lexer.Keywords)
            if ((string)keyword.value == node.Id)
                throw new Exception("Nombre de variable no permitido");

    }
    public void Visit_string(AST node)
    {
        if (node is String) return;

        if (node is Var var)
            if (Current_scope.Symbol_tab[var.Id] is Var_symbol var_Symbol)
                if (var_Symbol.type == "string") return;

        throw new Exception("Expresion incoherente en segun tipo");
    }

    public void Visit_Expr(AST node)
    {
        switch (node)
        {
            case Binary_op binary when !(binary.Op.type == "comparison") && binary.Op.type != "or" && binary.Op.type != "and":
                Visit_Expr(binary.Left);
                Visit_Expr(binary.Right);
                return;
            case Unitary_op unitary:
            {
                if (unitary.Op.type == "Minus") Visit_Expr(unitary.Right);
                else throw new Exception("Expresion incoherente en segun tipo");
                return;
            }
            case Var var:
                Visit_Var(var, "int");
                break;
            case Get get:
                VisitGet(get);
                break;
            case Integer:
                return;
            case MethodCall method:
                Visit_Method_Call(method, "int");
                break;
            default:
                throw new Exception("Expresion incoherente en segun tipo");
        }
    }


    public void Visit_Bexpr(AST node)
    {
        if (node is Binary_op binary)
        {
            if ((binary.Op.type == "Comparison"))
            {
                Visit_Expr(binary.Left);
                Visit_Expr(binary.Right);
                return;
            }
            else if (binary.Op.type == "or" || binary.Op.type == "and")
            {
                Visit_Bexpr(binary.Left);
                Visit_Bexpr(binary.Right);
                return;
            }
            else throw new Exception("Expresion incoherente en segun tipo");
        }
        else if (node is Bool) return;
        else if (node is Var var) Visit_Var(var, "bool");
        else if (node is MethodCall method) Visit_Method_Call(method, "bool");
        else throw new Exception("Expresion incoherente en segun tipo");
    }
}
