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
NEW: 'new';
STACKNEW: 'stacknew';

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
importStatement: IMPORT namespaceName semi=';';

// Import alias
importAlias: IMPORT IDENTIFIER AS namespaceName '.' typeReference semi=';';


// ### Declarations
// Namespace declaration
namespaceDeclaration: NAMESPACE namespaceName rootMemberBlock;

namespaceName: IDENTIFIER (':' IDENTIFIER)*;

// Type declaration
typeDeclaration: attributeDeclaration* accessModifier* TYPE IDENTIFIER genericParameterList? inheritParameters? memberBlock;

// Contract declaration
contractDeclaration: attributeDeclaration* accessModifier* CONTRACT IDENTIFIER genericParameterList? inheritParameters? memberBlock;

// Enum declaration
enumDeclaration: attributeDeclaration* accessModifier* ENUM IDENTIFIER (':' primitiveType)? enumBlock;

// Enum block
enumBlock: lblock='{' (enumField (',' enumField)*)? lblock='}';

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
	| accessorDeclaration
	| methodDeclaration
	;

// Attribute declaration
attributeDeclaration: '#' typeReference ('(' (expression (',' expression)*)? ')')?;

// Field declaration
fieldDeclaration: attributeDeclaration* accessModifier* typeReference IDENTIFIER fieldAssignment? ';';

fieldAssignment: assign='=' expression;

// Accessor declaration
accessorDeclaration: attributeDeclaration* accessModifier* typeReference IDENTIFIER accessorBody;

// Accessor body
accessorBody: 
	'=>' expression ';'
	| accessorWrite accessorRead?
	| accessorRead accessorWrite?
	;

// Accessor read
accessorRead: '=>' READ ':' (statement | statementBlock);

// Accessor write
accessorWrite: '=>' WRITE ':' (statement | statementBlock);

// Initializer declaration
initializerDeclaration: attributeDeclaration* accessModifier* THIS '(' methodParameterList? ')';

// Method declaration
methodDeclaration: attributeDeclaration* accessModifier* typeReference IDENTIFIER genericParameterList? '(' methodParameterList? ')' (';' | statementBlock);

// Method parameter list
methodParameterList: methodParameter (',' methodParameter)*;

// Method parameter
methodParameter: typeReference IDENTIFIER '...'?;

// Access modifiers
accessModifier: EXPORT | INTERNAL | SPECIALHIDDEN | GLOBAL;

// Generic parameters
genericParameterList: '<' genericParameter (',' genericParameter)* '>';

genericParameter: IDENTIFIER (':' typeReference (',' typeReference)*)?;

// Generic arguments
genericArguments: lgen='<' typeReference (',' typeReference)* rgen='>';

// Array parameters
arrayParameters: '[' ','? ','? ']';

// Inheritance
inheritParameters: ':' typeReference (',' typeReference)*;


// Type reference
typeReference: (primitiveType | ((IDENTIFIER ':')* parentTypeReference? IDENTIFIER genericArguments?)) arrayParameters? ref='&'?;
shortTypeReference: IDENTIFIER genericArguments? arrayParameters?;
parentTypeReference: (shortTypeReference '.')*;
primitiveType: ANY | BOOL | CHAR | I8 | U8 | I16 | U16 | I32 | U32 | I64 | U64 | FLOAT | DOUBLE | STRING;



// ### STATEMENTS
statementBlock: '{' statement* '}';

statement: 
	  returnStatement
	| methodInvokeStatement
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
returnStatement: RETURN expression? semi=';';

// Invoke statement
methodInvokeStatement: expression? methodInvokeExpression semi=';';

// Postfix statement
postfixStatement: expression ('++' | '--') ';';

// Local variable statement
localVariableStatement: typeReference IDENTIFIER (',' IDENTIFIER)* localVariableAssignment? semi=';';

localVariableAssignment: assign='=' (expression | lblock='{' expression (',' expression)* rblock='}');

// Assignment statement
assignStatement: expression assign=('=' | '+=' | '-=' | '/=' | '*=') expression semi=';';

// If statement
ifStatement: IF lparen='(' expression rparen=')' (statement | semi=';' | statementBlock) elseifStatement* elseStatement?;

// Elseif statement
elseifStatement: ELSEIF lparen='(' expression rparen=')' (statement | semi=';' | statementBlock);

// Else statement
elseStatement: ELSE (statement | semi=';' | statementBlock);

// Foreach statement
foreachStatement: FOREACH lparen='(' typeReference IDENTIFIER IN expression rparen=')' (statement? semi=';' | statementBlock);

// For statement
forStatement: FOR lparen='(' (forVariableStatement (',' forVariableStatement)*)? semi=';' expression? semi=';' (forIncrementStatement (',' forIncrementStatement)*)? rparen=')' (statement? semi=';' | statementBlock);
forVariableStatement: typeReference IDENTIFIER ('=' expression)?;
forIncrementStatement: expression;

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

	  unary='-' expression										// Unary minus
	| unary='!' expression										// not expression
	| unary='++' expression										// Unary prefix increment
	| unary='--' expression										// Unary prefix decrement
	| expression unary='++'										// Unary postfix increment
	| expression unary='--'										// Unary postfix decrement
	| expression binary=('*' | '/' | '%') expression			// Multiply expression
	| expression binary=('+' | '-') expression					// Add expression
	| expression binary=('>=' | '<=' | '>' | '<') expression	// Compare expression
	| expression binary=('==' | '!=') expression				// Equals expression
	| expression binary='&&' expression							// And expression
	| expression binary='||' expression							// Or expression
	| expression ternary='?' expression alternate=':' expression			// Ternary expression
	| IDENTIFIER indexExpression*
	| endExpression indexExpression*							// Primitives and literals
	| expression methodInvokeExpression indexExpression*		// Method expression
	| typeReference methodInvokeExpression indexExpression*		// Global method
	| expression fieldAccessExpression indexExpression*			// Field expression
	| typeReference fieldAccessExpression indexExpression*		// Gloabl field
	| lparen='(' expression lparen=')' indexExpression*			// Paren expression
	| typeExpression											// Type expression
	| sizeExpression											// Size expression
	| newExpression indexExpression*
	| initializerInvokeExpression indexExpression*
	| THIS indexExpression*
	| BASE indexExpression*
	| typeReference
	;

endExpression: 
	  HEX 
	| INT decorator=('U' | 'L' | 'UL')? 
	| DECIMAL decorator=('F' | 'D')? 
	| LITERAL
	//| IDENTIFIER 
	| TRUE 
	| FALSE
	| NULL
	;

typeExpression: TYPE lparen='(' typeReference rparen=')';

sizeExpression: SIZE lparen='(' typeReference rparen=')';

newExpression: (NEW | STACKNEW) initializerInvokeExpression;

// Array index
indexExpression: larray='[' expression (',' expression)* rarray=']';

// Field access
fieldAccessExpression: dot='.' IDENTIFIER;

// Method invoke
methodInvokeExpression: dot='.'? IDENTIFIER genericArguments? lparen='(' methodArgument? (',' methodArgument)* rparen=')';
methodArgument: '&'? expression;

// Initializer 
initializerInvokeExpression: typeReference lparen='(' (expression (',' expression)*)? rparen=')';
