using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AST
{
    //not usefull
}

public class Empty : AST
{
    //pass
}
public class String : AST
{
    public string Value;
    public String(string value)
    {
        Value = value;
    }
}
public class Var : AST
{
    public string Id;
    public Var(string ID)
    {
        this.Id = ID;
    }
}
public class Integer : AST
{
    public int Value;
    public Integer(int value)
    {
        this.Value = value;
    }
}
public class Binary_op : AST
{
    public AST Left;
    public Token Op;
    public AST Right;

    public Binary_op(AST left, Token op, AST right)
    {
        Left = left;
        Op = op;
        Right = right;
    }
}
public class Unitary_op : AST
{
    public Token Op;
    public AST Right;

    public Unitary_op(Token op, AST right)
    {
        Op = op;
        Right = right;
    }
}
public class Bool : AST
{
    public bool Value;
    public Bool(bool value)
    {
        this.Value = value;
    }
}

public class If : AST
{
    public AST Condition;
    public Block Block;

    public If(AST condition, Block block)
    {
        Condition = condition;
        Block = block;
    }
}
public class While : AST
{
    public AST Condition;
    public Block Block;

    public While(AST condition, Block block)
    {
        Condition = condition;
        Block = block;
    }
}
public class Assign : AST
{
    public string Type;
    public Var Var;
    public AST Expr;

    public Assign(Var var, AST expr)
    {
        Type = "";
        Var = var;
        Expr = expr;
    }
}
public class VariableD : AST
{
    public string Type;
    public List<Var> IDs;

    public VariableD(string type)
    {
        Type = type;
        IDs = new List<Var>();
    }
}
public class MethodD : AST
{
    public string Type;
    public string Id;
    public Parameters Parameters;
    public Block Block;

    public MethodD(string type, string id, Parameters parameters, Block block)
    {
        Type = type;
        Id = id;
        Parameters = parameters;
        Block = block;
    }
}
public class Return : AST
{
    public AST Retorno;

    public Return(AST retorno)
    {
        Retorno = retorno;
    }
}
public class MethodCall : AST
{
    public string Id;
    public List<AST> ActualParameters;
    public Method_symbol MethodReference;
    
    public MethodCall(string id, List<AST> actualParameters)
    {
        Id = id;
        ActualParameters = actualParameters;
    }
}


public class Block : AST
{
    public List<AST> SList;
    public Block(List<AST> s_list) => SList = s_list;
}
public class Parameters : AST
{//se puede prescindir de esta clase
    public List<(string, string)> parameters;
    // Type-Value
    public Parameters(List<(string, string)> parameters) => this.parameters = parameters;
}

//clases Power
public class TargetedPower : AST
{
    public AST Chage;

    public TargetedPower(AST chage)
    {
        Chage = chage;
    }
}
public class RandomPower : AST
{
    public Card[,] Field;
    public string Place;
    public AST Change;
    public AST Targets;

    public RandomPower(string place, AST targets, AST change)
    {
        Place = place;
        Change = change;
        Targets = targets;
    }
}

public class SelfPower : AST
{
    public AST Change;

    public SelfPower(AST change)
    {
        Change = change;
    }
}

public class PosPower : AST
{
    public List<string> Directions;
    public AST Change;
    public PosPower(List<string> directions, AST change)
    {
        Directions = directions;
        Change = change;
    }
}

public class SelectPower : AST
{
    public Card[,] Field;
    public string Place;
    public string Stat;
    public AST Change;

    public SelectPower(string place, string stat, AST change)
    {
        Place = place;
        Stat = stat;
        Change = change;
    }
}
public class Destroy : AST
{
    //pass
}

public class OriginalPower : AST
{
    //
}

public class OriginalSelectPower : AST
{
    public string Place;
    public Card[,] Field;
    public string Stat;
    public OriginalSelectPower(string place, string stat)
    {
        Place = place;
        Stat = stat;
    }
}
//clase Get
public class Get : AST
{
    public string Stat;
    public string Place;
    public Card[,] Field;

    public Get(string stat, string place)
    {
        Stat = stat;
        Place = place;
    }
}
//clases Summon