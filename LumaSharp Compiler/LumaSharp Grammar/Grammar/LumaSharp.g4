grammar LumaSharp;

// Lexer keywords
IMPORT: 'import';
NAMESPACE: 'namespace';
TYPE: 'type';
ENUM: 'enum';

GLOBAL: 'global';
EXPORT: 'export';
INTERNAL: 'internal';

// Lexer rules
ID: [a-zA-Z_][a-zA-Z0-9_]*;
INT: [0-9]+;
STRING: '"' .*? '"';
WS: [ \t\r\n]+ -> skip;

// Lexer block
lb: '{';
rb: '}';
lp: '(';
rp: ')';
la: '[';
ra: ']';
lg: '<';
rg: '>';





// Parser rules
compilationUnit: (namespaceDeclaration | typeDeclaration | enumDeclaration)*;

namespaceDeclaration: NAMESPACE ID lb rb | NAMESPACE ID lb typeDeclaration* rb;

typeDeclaration: TYPE ID lb memberDeclaration* rb;

enumDeclaration: ENUM ID lb enumField* rb;

enumField: ID ('=' INT)? ',';

memberDeclaration: typeDeclaration | fieldDeclaration;

fieldDeclaration: accessModifier? GLOBAL? primitiveType ID fieldAssignment? ';';

fieldAssignment: '=' (INT | STRING | ID);

accessModifier: EXPORT | INTERNAL;

primitiveType: 'i8' | 'u8' | 'i16' | 'u16' | 'i32' | 'u32' | 'i64' | 'u64' | 'float' | 'double' | 'string';

// This rule is optional and will be used to ignore comments
COMMENT: '/*' .*? '*/' -> skip;