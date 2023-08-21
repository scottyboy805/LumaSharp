grammar LumaSharp;

// Lexer keywords
IMPORT: 'import';
NAMESPACE: 'namespace';
TYPE: 'type';
CONTRACT: 'contract';
ENUM: 'enum';

// Access modifier keywords
GLOBAL: 'global';
EXPORT: 'export';
INTERNAL: 'internal';

// Other keywords
AS: 'as';
CONTINUE: 'continue';
BREAK: 'break';
RETURN: 'return';
IF: 'if';
ELSE: 'else';
ELSEIF: 'elseif';
TRUE: 'true';
FALSE: 'false';

// Primitive type keywords
I8: 'i8';
U8: 'u8';
I16: 'i16';
U16: 'u16';
I32: 'i32';
U32: 'u32';
I64: 'i64';
U64: 'u64';
FLOAT: 'float';
DOUBLE: 'double';
STRING: 'string';

// Lexer rules
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;
INT: [0-9]+;
DECIMAL: [0-9]+ '.' [0-9]+;
LITERAL: '"' .*? '"';
WS: [ \t\r\n]+ -> skip;

// This rule is optional and will be used to ignore comments
COMMENT: '/*' .*? '*/' -> skip;



// Parser rules
// Compilation unit - root of a source file
compilationUnit: (importStatement | importAlias)* (namespaceDeclaration | typeDeclaration | contractDeclaration | enumDeclaration)*;

// Import statement
importStatement: IMPORT IDENTIFIER ('.' IDENTIFIER)* ';';

// Import alias
importAlias: IMPORT IDENTIFIER AS IDENTIFIER ('.' IDENTIFIER)+ ';';


// ### Declarations
// Namespace declaration
namespaceDeclaration: NAMESPACE IDENTIFIER '{' (typeDeclaration | contractDeclaration | enumDeclaration)* '}';

// Type declaration
typeDeclaration: attributeDeclaration* accessModifier? TYPE IDENTIFIER genericParameters? inheritParameters? '{' (typeDeclaration | contractDeclaration | enumDeclaration)* '}';

// Contract declaration
contractDeclaration: attributeDeclaration* accessModifier? CONTRACT IDENTIFIER genericParameters? inheritParameters? '{' (enumDeclaration | methodDeclaration)* '}';

// Enum declaration
enumDeclaration: attributeDeclaration* accessModifier? ENUM IDENTIFIER (':' primitiveType)? '{' enumFields? '}';
enumFields: enumField (',' enumField)*;
enumField: IDENTIFIER ('=' INT)?;

// Attribute declaration
attributeDeclaration: '#' IDENTIFIER ('(' ((INT | DECIMAL | LITERAL) (',' (INT | DECIMAL | LITERAL))*)? ')')?;

fieldDeclaration: accessModifier? GLOBAL? primitiveType IDENTIFIER fieldAssignment? ';';

fieldAssignment: '=' (INT | STRING | IDENTIFIER);

methodDeclaration: accessModifier? GLOBAL? primitiveType IDENTIFIER '(' ')';

// Access modifiers
accessModifier: EXPORT | INTERNAL;

// Generic parameters
genericParameters: '<' IDENTIFIER (',' IDENTIFIER)* '>';

// Inheritance
inheritParameters: ':' IDENTIFIER (',' IDENTIFIER)*;


// Type reference
typeReference: primitiveType | (IDENTIFIER ('.' IDENTIFIER)* genericParameters?);
primitiveType: I8 | U8 | I16 | U16 | I32 | U32 | I64 | U64 | FLOAT | DOUBLE | STRING;



// ### STATEMENTS
statement: 
	  returnStatement
	| blockStatement
	| localVariableStatement
	| ifStatement;

// Block statement
blockStatement: '{' statement* '}';

// Return statement
returnStatement: RETURN expression? ';';

// Local variable statement
localVariableStatement: typeReference IDENTIFIER ('=' expression)? ';';

// If statement
ifStatement: IF '(' expression ')' (statement? ';' | '{' statement* '}') elseifStatement* elseStatement?;

// Elseif statement
elseifStatement: ELSEIF '(' expression ')' (statement? ';' | '{' statement* '}');

// Else statement
elseStatement: ELSE (statement? ';' | '{' statement* '}');

// ### EXPRESSIONS
expression: (endExpression);

endExpression: INT | DECIMAL | LITERAL | IDENTIFIER | TRUE | FALSE;