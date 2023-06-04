module dandeiion.Codegen

type ICodegen =
    abstract member setType: Ast.StructTypes -> unit
