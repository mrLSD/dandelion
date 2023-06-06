module dandeiion.Codegen

type ICodegen =
    abstract member setType: Ast.StructTypes -> unit
    abstract member setConstant: Ast.Constant -> unit
    abstract member setFunction: Ast.FunctionStatement -> unit
