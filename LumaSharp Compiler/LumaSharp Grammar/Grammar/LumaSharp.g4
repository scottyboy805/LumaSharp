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
SPECIALHIDDEN: 'hidden';

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
FOREACH: 'foreach';
IN: 'in';
FOR: 'for';
WHILE: 'while';
SELECT: 'select';
MATCH: 'match';
DEFAULT: 'default';
TRY: 'try';
CATCH: 'catch';
FINALLY: 'finally';
SIZE: 'size';
READ: 'read';
WRITE: 'write';
THIS: 'this';
BASE: 'base';

// Primitive type keywords
ANY: 'any';
BOOL: 'bool';
CHAR: 'char';
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
NULL: 'null';

// Lexer rules
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;
INT: [0-9]+;
DECIMAL: [0-9]+ '.' [0-9]+;
HEX: '0x' [0-9a-fA-F]+;
LITERAL: '"' .*? '"';
WS: [ \t\r\n]+ -> channel(HIDDEN);

// This rule is optional and will be used to ignore comments
COMMENT: '/*' .*? '*/' -> skip;



// Parser rules
// Compilation unit - root of a source file
compilationUnit: importElement* rootElement*;

// Import element
importElement: importStatement | importAlias;

// Root element
rootElement: namespaceDeclaration | rootMember;

// Import statement
importStatement: IMPORT namespaceName ';';

// Import alias
importAlias: IMPORT IDENTIFIER AS namespaceName '.' typeReference ';';


// ### Declarations
// Namespace declaration
namespaceDeclaration: NAMESPACE namespaceName rootMemberBlock;

namespaceName: IDENTIFIER (':' IDENTIFIER)*;

// Type declaration
typeDeclaration: attributeDeclaration* accessModifier* TYPE IDENTIFIER genericParameters? inheritParameters? memberBlock;

// Contract declaration
contractDeclaration: attributeDeclaration* accessModifier* CONTRACT IDENTIFIER genericParameters? inheritParameters? memberBlock;

// Enum declaration
enumDeclaration: attributeDeclaration* accessModifier* ENUM IDENTIFIER (':' primitiveType)? enumBlock;

// Enum block
enumBlock: '{' (enumField (',' enumField)*)? '}';

// Enum field
enumField: attributeDeclaration* IDENTIFIER fieldAssignment?;

// Declaration block
rootMember: (typeDeclaration | contractDeclaration | enumDeclaration);

rootMemberBlock: '{' rootMember* '}';

// Member block
memberBlock: '{' memberDeclaration* '}';

// Member
memberDeclaration: 
	  typeDeclaration 
	| contractDeclaration 
	| enumDeclaration 
	| fieldDeclaration 
	| methodDeclaration
	;

// Attribute declaration
attributeDeclaration: '#' typeReference ('(' (expression (',' expression)*)? ')')?;

// Field declaration
fieldDeclaration: attributeDeclaration* accessModifier* typeReference IDENTIFIER fieldAssignment? ';';

fieldAssignment: '=' (INT | STRING | IDENTIFIER);

// Accessor declaration
accessorDeclaration: attributeDeclaration* accessModifier* typeReference IDENTIFIER accessorBody;

// Accessor body
accessorBody: 
	  '=>' endExpression ';'
	| '=>' IDENTIFIER ';'
	| accessorRead
	| accessorWrite
	| accessorRead accessorWrite
	| accessorWrite accessorRead
	;

// Accessor read
accessorRead: '=>' READ ':' (statement | statementBlock);

// Accessor write
accessorWrite: '=>' WRITE ':' (statement | statementBlock);

// Initializer declaration
initializerDeclaration: attributeDeclaration* accessModifier* THIS '(' methodParameterList? ')';

// Method declaration
methodDeclaration: attributeDeclaration* accessModifier* typeReference IDENTIFIER '(' methodParameterList? ')';

// Method parameter list
methodParameterList: methodParameter (',' methodParameter)*;

// Method parameter
methodParameter: typeReference IDENTIFIER '...'?;

// Access modifiers
accessModifier: EXPORT | INTERNAL | SPECIALHIDDEN | GLOBAL;

// Generic parameters
genericParameters: '<' IDENTIFIER (',' IDENTIFIER)* '>';

// Generic arguments
genericArguments: '<' typeReference (',' typeReference)* '>';

// Array parameters
arrayParameters: '[' ','? ','? ']';

// Inheritance
inheritParameters: ':' typeReference (',' typeReference)*;


// Type reference
typeReference: (primitiveType | (IDENTIFIER ('.' IDENTIFIER)* genericArguments?)) arrayParameters? '&'?;
primitiveType: ANY | BOOL | CHAR | I8 | U8 | I16 | U16 | I32 | U32 | I64 | U64 | FLOAT | DOUBLE | STRING;



// ### STATEMENTS
statementBlock: '{' statement* '}';

statement: 
	  returnStatement
	| postfixStatement
	| statementBlock
	| localVariableStatement
	| assignStatement
	| ifStatement
	| foreachStatement
	| forStatement
	| whileStatement
	| selectStatement
	| tryStatement
	| BREAK ';'
	| CONTINUE ';';

// Return statement
returnStatement: RETURN expression? ';';

// Postfix statement
postfixStatement: expression ('++' | '--') ';';

// Local variable statement
localVariableStatement: typeReference IDENTIFIER ('=' expression)? ';';

// Assignment statement
assignStatement: expression ('=' | '+=' | '-=' | '/=' | '*=') expression ';';

// If statement
ifStatement: IF '(' expression ')' (statement | ';' | '{' statement* '}') elseifStatement* elseStatement?;

// Elseif statement
elseifStatement: ELSEIF '(' expression ')' (statement | ';' | '{' statement* '}');

// Else statement
elseStatement: ELSE (statement | ';' | '{' statement* '}');

// Foreach statement
foreachStatement: FOREACH '(' typeReference IDENTIFIER IN expression ')' (statement? ';' | '{' statement* '}');

// For statement
forStatement: FOR '(' forVariableStatement? ';' expression? ';' expression? ')' (statement? ';' | '{' statement* '}');
forVariableStatement: typeReference IDENTIFIER ('=' expression)?;

// While statement
whileStatement: WHILE '(' expression ')' (statement | ';' | '{' statement* '}');

// Select statement
selectStatement: SELECT '(' expression ')' '{' (defaultStatement | matchStatement)* '}';
defaultStatement: DEFAULT ':' (statement | ';' | '{' statement* '}');
matchStatement: MATCH expression ':' (statement | ';' | '{' statement* '}');

// Try statement
tryStatement: TRY (statement | '{' statement* '}') catchStatement? finallyStatement?;
catchStatement: CATCH ('(' typeReference ')')? (statement | '{' statement* '}');
finallyStatement: FINALLY (statement | '{' statement* '}');


// ### EXPRESSIONS
expression: //(endExpression | arrayIndexExpression | fieldAccessExpression);

	  '-' expression										// Unary minus
	| '!' expression										// not expression
	//| '++' expression										// Unary prefix increment
	//| '--' expression										// Unary prefix decrement
	//| expression '++'										// Unary postfix increment
	//| expression '--'										// Unary postfix decrement
	| expression op=('*' | '/' | '%') expression			// Multiply expression
	| expression op=('+' | '-') expression					// Add expression
	| expression op=('>=' | '<=' | '>' | '<') expression	// Compare expression
	| expression op=('==' | '!=') expression				// Equals expression
	| expression '&&' expression							// And expression
	| expression '||' expression							// Or expression
	| expression '?' expression ':' expression				// Ternary expression
	| endExpression indexExpression?						// Primitives and literals
	| expression methodInvokeExpression indexExpression?		// Method expression
	| expression fieldAccessExpression indexExpression?		// Field expression
	| '(' expression ')' indexExpression?					// Paren expression
	| typeExpression										// Type expression
	| sizeExpression										// Size expression
	;

endExpression: 
	  HEX 
	| INT ('U' | 'L' | 'UL')? 
	| DECIMAL ('F' | 'D')? 
	| LITERAL
	| IDENTIFIER 
	| TRUE 
	| FALSE
	| NULL
	;

typeExpression: TYPE '(' typeReference ')';

sizeExpression: SIZE '(' typeReference ')';

// Array index
indexExpression: '[' expression ']';

// Field access
fieldAccessExpression: '.' IDENTIFIER;

// Method invoke
methodInvokeExpression: '.' IDENTIFIER genericArguments? '(' methodArgument? (',' methodArgument)* ')';
methodArgument: '&'? expression;
