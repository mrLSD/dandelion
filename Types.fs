module dandeiion.Types
(* 
type ValueName = ValueName of string
type InnerValueName = InnerValueName of string

type PrimitiveTypes =
    | U8 
    | U16 
    | U32 
    | U64
    | I8
    | I16
    | I32
    | I64
    | F32
    | F64
    | Bool
    | Char
    | String
    | None
    member this.getName =
        match this with
        | U8 -> "u8"
        | U16 -> "u16"
        | U32 -> "u32"
        | U64 -> "u64"
        | I8 -> "i8"
        | I16 -> "i16"
        | I32 -> "i32"
        | I64 -> "i64"
        | F32 -> "f32"
        | F64 -> "f64"
        | Bool -> "bool"
        | Char -> "char"
        | String -> "string"
        | None -> "()"

type Type =
    | Primitive of PrimitiveTypes
    | Struct of StructTypes
    | Array of Type * uint32
    member this.getName =
        ""

and StructType = {
    attrName: Ident
    attrType: Type
}

and StructTypes =
    {
        name: Ident
        types: StructType[]
    }
    member this.getName =
        this.name.span


type Type =
    | Primitive of PrimitiveTypes
    | Struct of StructTypes
    | Array of Type * uint32



[<Struct>]
type Value = {
    innerName: ValueName
    innerValueName: InnerValueName
    innerType: Type
    isMutable: bool
    isAlloca: bool
    isMalloc: bool
}

type StateErrorKind = 
    | ConstantAlreadyExist
    | TypeAlreadyExist
    | FunctionAlreadyExist
    | ValueNotFound
    | FunctionNotFound
    | ReturnNotFound
    

type StateErrorLocation = {
    line: uint64
    column: uint64
}

type StateErrorResult = {
    kind: StateErrorKind
    value: string
    location: StateErrorLocation
}
type StateResult<'T> = Result<'T, StateErrorResult>
type StateResults<'T> = Result<'T, StateErrorResult[]>
*)