grammar LumaSharp;

// Lexer keywords
IMPORT: 'import';
NAMESPACE: 'namespace';
TYPE: 'type';
CONTRACT: 'contract';
ATTRIBUTE: 'attribute';
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
OVERRIDE: 'override';
IF: 'if';
ELSE: 'else';
ELSEIF: 'elseif';
TRUE: 'true';
FALSE: 'false';
IN: 'in';
FOR: 'for';
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
F32: 'f32';
F64: 'f64';
STRING: 'string';
NULL: 'null';
VOID: 'void';

// Symbols
LGENERIC: '<';
RGENERIC: '>';
LARRAY: '[';
RARRAY: ']';
LBLOCK: '{';
RBLOCK: '}';
LPAREN: '(';
RPAREN: ')';

DOT: '.';
COMMA: ',';
COLON: ':';
HASH: '#';
ASSIGN: '=';
LAMBDA: '=>';
ENUMERABLE: '...';
TERNARY: '?';

RANGEINCLUSIVE: '..=';
RANGEEXCLUSIVE: '..<';

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
compilationUnit: 
	importElement* 
	rootElement*;

// Import element
importElement: 
	importStatement | 
	importAlias;

// Root element
rootElement: 
	namespaceDeclaration | 
	rootMember;

// Import statement
importStatement: 
	IMPORT 
	namespaceName;

// Import alias
importAlias: 
	IMPORT 
	IDENTIFIER 
	AS 
	namespaceName 
	DOT 
	typeReference;


// ### Declarations
// Namespace declaration
namespaceDeclaration: 
	NAMESPACE 
	namespaceName;

namespaceName: 
	IDENTIFIER 
	namespaceNameSecondary*;

namespaceNameSecondary:
	COLON
	IDENTIFIER;

// Type declaration
typeDeclaration: 
	attributeReference* 
	accessModifier* 
	(TYPE | ATTRIBUTE) 
	IDENTIFIER 
	genericParameterList? 
	OVERRIDE?
	inheritParameters? 
	memberBlock;

// Contract declaration
contractDeclaration: 
	attributeReference* 
	accessModifier* 
	CONTRACT 
	IDENTIFIER 
	genericParameterList? 
	inheritParameters? 
	memberBlock;

// Enum declaration
enumDeclaration: 
	attributeReference* 
	accessModifier* 
	ENUM 
	IDENTIFIER 
	(COLON primitiveType)? 
	enumBlock;

// Enum block
enumBlock: 
	LBLOCK 
	enumField*
	RBLOCK;

// Enum field
enumField: 
	attributeReference* 
	IDENTIFIER 
	variableAssignment?
	COMMA?;

// Declaration block
rootMember: 
	(typeDeclaration | 
	contractDeclaration | 
	enumDeclaration);

rootMemberBlock: 
	LBLOCK 
	rootMember* 
	RBLOCK;

// Member block
memberBlock: 
	LBLOCK 
	memberDeclaration* 
	RBLOCK;

// Member
memberDeclaration: 
	  typeDeclaration 
	| contractDeclaration 
	| enumDeclaration 
	| fieldDeclaration 
	| accessorDeclaration
	| methodDeclaration;

attributeReference: 
	HASH 
	typeReference 
	argumentList?;

// Field declaration
fieldDeclaration: 
	attributeReference* 
	accessModifier* 
	typeReference 
	IDENTIFIER 
	variableAssignment?
	COMMA?;

// Accessor declaration
accessorDeclaration: 
	attributeReference* 
	accessModifier* 
	typeReference 
	IDENTIFIER 
	OVERRIDE?
	accessorBody;

// Accessor body
accessorBody: 
	expressionLambda
	| accessorReadWrite accessorReadWrite?;	// Allow for 2 but require 1

// Accessor read
accessorReadWrite: 
	LAMBDA 
	(READ | WRITE)
	COLON
	(statement COMMA? | statementBlock);

// Initializer declaration
initializerDeclaration: 
	attributeReference* 
	accessModifier* 
	THIS 
	LPAREN 
	methodParameterList?
	RPAREN
	initializerBaseExpression?
	statementBlock;

initializerBaseExpression:
	COLON
	LPAREN
	expressionList
	RPAREN;

// Method declaration
methodDeclaration: 
	attributeReference* 
	accessModifier* 
	methodReturnList 
	IDENTIFIER 
	genericParameterList? 
	methodParameterList 
	OVERRIDE?
	(statementLambda | statementBlock)?;

// Method return list
methodReturnList:
	VOID |
	typeReferenceList;

// Method parameter list
methodParameterList: 
	LPAREN
	(methodParameter 
	methodParameterSecondary*)?
	RPAREN;

// Method parameter
methodParameter: 
	attributeReference*
	typeReference 
	IDENTIFIER 
	ENUMERABLE?
	(ASSIGN expression)?;

methodParameterSecondary:
	COMMA
	methodParameter;

// Access modifiers
accessModifier: 
	EXPORT 
	| INTERNAL 
	| SPECIALHIDDEN 
	| GLOBAL;



// Generic parameters
genericParameterList: 
	LGENERIC						// <
	genericParameter
	genericParameterSecondary* 
	RGENERIC;						// >

genericParameter: 
	IDENTIFIER						// T
	genericConstraintList?;

genericParameterSecondary:
	COMMA
	genericParameter;

genericConstraintList:
	COLON
	genericConstraint
	genericConstraintSecondary*;

genericConstraint:
	typeReference;

genericConstraintSecondary:
	COMMA
	genericConstraint;



// Generic arguments
genericArgumentList: 
	LGENERIC						// <
	typeReferenceList?
	RGENERIC;						// >


// Array parameters
arrayParameters: 
	LARRAY 
	COMMA? 
	COMMA? 
	RARRAY;

// Inheritance
inheritParameters: 
	COLON 
	typeReferenceList;


// Type reference
typeReferenceList:
	typeReference
	typeReferenceSecondary*;

typeReference: 
	(primitiveType 
	| (namespaceName? parentTypeReference* IDENTIFIER genericArgumentList?)) 
	arrayParameters?;

typeReferenceSecondary:
	COMMA
	typeReference;

shortTypeReference: 
	IDENTIFIER 
	genericArgumentList? 
	arrayParameters?;

parentTypeReference: 
	IDENTIFIER
	genericArgumentList? 
	DOT;

primitiveType: 
	ANY 
	| BOOL 
	| CHAR 
	| I8 
	| U8 
	| I16 
	| U16 
	| I32 
	| U32 
	| I64 
	| U64 
	| F32 
	| F64 
	| STRING
	| VOID;



// ### STATEMENTS
statementBlock: 
	LBLOCK 
	statement*
	RBLOCK;

statementLambda:
	LAMBDA
	statement
	COMMA?;

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
	| selectStatement
	| tryStatement
	| BREAK
	| CONTINUE;

// Return statement
returnStatement: 
	RETURN 
	expressionList?;

// Invoke statement
methodInvokeStatement: 
	expression? 
	methodInvokeExpression;

// Postfix statement
postfixStatement: 
	expression 
	operand=('++' | '--');

// Local variable statement
localVariableStatement: 
	typeReference 
	IDENTIFIER 
	localVariableSecondary* 
	variableAssignment?;

localVariableSecondary:
	COMMA
	IDENTIFIER;

variableAssignment: 
	ASSIGN 
	(expression
	| LBLOCK expressionList RBLOCK);

// Assignment statement
assignStatement: expressionList assign=('=' | '+=' | '-=' | '/=' | '*=') expressionList;

// If statement
ifStatement: 
	IF 
	expression
	(statement | statementBlock) 
	elseifStatement* 
	elseStatement?;

// Elseif statement
elseifStatement: 
	ELSEIF 
	expression 
	(statement | statementBlock);

// Else statement
elseStatement: 
	ELSE 
	(statement | statementBlock);

// For statement
forStatement: 
	FOR
	localVariableStatement?
	COLON
	expression?
	COLON
	expressionList?
	(statement | statementBlock);

foreachStatement: 
	FOR
	typeReference 
	IDENTIFIER 
	IN 
	(rangeExpression | expression)
	(statement | statementBlock);

// Select statement
selectStatement: 
	SELECT 
	expression 
	LBLOCK 
	(defaultStatement | matchStatement)*
	RBLOCK;

defaultStatement: 
	DEFAULT 
	COLON 
	(statement | LBLOCK statement* RBLOCK);

matchStatement: 
	MATCH
	expression 
	COLON 
	(statement | LBLOCK statement* RBLOCK);

// Try statement
tryStatement: 
	TRY 
	(statement | LBLOCK statement* RBLOCK) 
	catchStatement? 
	finallyStatement?;

catchStatement: 
	CATCH (LPAREN typeReference RPAREN)? 
	(statement | LBLOCK statement* RBLOCK);

finallyStatement: 
	FINALLY 
	(statement | LBLOCK statement* RBLOCK);


// ### EXPRESSIONS
expressionList:
	expression
	expressionSecondary*;

expressionLambda:
	LAMBDA
	expression
	COMMA?;

expression:
	  unaryPrefix=('-' | '!' | '++' | '--') expression							// Unary prefix decrement
	| expression unaryPostfix=('++' | '--')				// Unary postfix decrement
	| expression binary=('*' | '/' | '%') expression				// Multiply expression
	| expression binary=('>=' | '<=' | '>' | '<' | '==' | '!=') expression					// Add expression
	| expression binary=('&&' | '||') expression				// And expression
	| expression TERNARY expression COLON expression
	| expression indexExpression						// Array index
	| IDENTIFIER
	| endExpression										// Primitive and literals
	| methodInvokeExpression							// Method invoke
	| fieldAccessExpression								// Field expression
	| parenExpression									// Paren expression
	| typeExpression									// Type expression
	| sizeExpression									// Size expression
	| newExpression										// New expression
	| THIS
	| BASE
	| typeReference;		

parenExpression:
	LPAREN expression RPAREN;

expressionSecondary:
	COMMA
	expression;

endExpression: 
	  HEX 
	| INT decorator=('U' | 'L' | 'UL')? 
	| DECIMAL decorator=('F' | 'D')? 
	| LITERAL
	//| IDENTIFIER 
	| TRUE 
	| FALSE
	| NULL;

typeExpression: 
	TYPE 
	LPAREN 
	typeReference 
	RPAREN;

sizeExpression: 
	SIZE 
	LPAREN 
	typeReference 
	RPAREN;

newExpression: 
	NEW
	typeReference
	argumentList?;

// Array index
indexExpression: 
	LARRAY 
	expressionList
	RARRAY;

// Field access
fieldAccessExpression: 
	DOT 
	IDENTIFIER;

// Method invoke
methodInvokeExpression: 
	DOT? 
	IDENTIFIER 
	genericArgumentList? 
	argumentList;

// Argument list
argumentList:
	LPAREN
	expressionList? 
	RPAREN;

rangeExpression: 
	expression 
	(RANGEINCLUSIVE | RANGEEXCLUSIVE) 
	expression;
