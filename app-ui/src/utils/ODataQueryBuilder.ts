/**
 * Represents the OData query options for sorting, filtering, and paging
 */
export interface ODataQueryOptions {
  filter?: FilterExpression;
  orderBy?: OrderByClause[];
  top?: number;
  skip?: number;
  count?: boolean;
}

/**
 * Represents a single ordering clause
 */
export interface OrderByClause {
  property: string;
  direction: OrderDirection;
}

/**
 * Order direction enumeration
 */
export enum OrderDirection {
  Ascending = 'asc',
  Descending = 'desc'
}

/**
 * Base interface for all filter expressions
 */
export interface FilterExpression {
  type: 'binary' | 'unary' | 'function' | 'property' | 'literal';
}

/**
 * Represents a binary operation (e.g., eq, ne, gt, lt, and, or)
 */
export interface BinaryExpression extends FilterExpression {
  type: 'binary';
  left: FilterExpression;
  operator: BinaryOperator;
  right: FilterExpression;
}

/**
 * Represents a unary operation (e.g., not)
 */
export interface UnaryExpression extends FilterExpression {
  type: 'unary';
  operator: UnaryOperator;
  operand: FilterExpression;
}

/**
 * Represents a function call (e.g., contains, startswith, endswith)
 */
export interface FunctionExpression extends FilterExpression {
  type: 'function';
  functionName: string;
  arguments: FilterExpression[];
}

/**
 * Represents a property/field reference
 */
export interface PropertyExpression extends FilterExpression {
  type: 'property';
  propertyName: string;
}

/**
 * Represents a literal value (string, number, boolean, null)
 */
export interface LiteralExpression extends FilterExpression {
  type: 'literal';
  value: string | number | boolean | null | Date;
  literalType: LiteralType;
}

/**
 * Binary operators supported in OData filters
 */
export enum BinaryOperator {
  Equal = 'eq',
  NotEqual = 'ne',
  GreaterThan = 'gt',
  GreaterThanOrEqual = 'ge',
  LessThan = 'lt',
  LessThanOrEqual = 'le',
  And = 'and',
  Or = 'or',
  Add = 'add',
  Subtract = 'sub',
  Multiply = 'mul',
  Divide = 'div',
  Modulo = 'mod'
}

/**
 * Unary operators supported in OData filters
 */
export enum UnaryOperator {
  Not = 'not',
  Negate = '-'
}

/**
 * Literal value types
 */
export enum LiteralType {
  String = 'string',
  Number = 'number',
  Boolean = 'boolean',
  Null = 'null',
  DateTime = 'datetime',
  Guid = 'guid'
}

/**
 * Builder class for generating OData query strings from object representations
 */
export class ODataQueryBuilder {
  private options: ODataQueryOptions;

  constructor(options: ODataQueryOptions = {}) {
    this.options = options;
  }

  /**
   * Sets the filter expression
   */
  filter(expression: FilterExpression): ODataQueryBuilder {
    this.options.filter = expression;
    return this;
  }

  /**
   * Sets the orderBy clauses
   */
  orderBy(clauses: OrderByClause[]): ODataQueryBuilder {
    this.options.orderBy = clauses;
    return this;
  }

  /**
   * Sets the top (limit) value
   */
  top(value: number): ODataQueryBuilder {
    this.options.top = value;
    return this;
  }

  /**
   * Sets the skip value
   */
  skip(value: number): ODataQueryBuilder {
    this.options.skip = value;
    return this;
  }

  /**
   * Sets the count flag
   */
  count(value: boolean): ODataQueryBuilder {
    this.options.count = value;
    return this;
  }

  /**
   * Builds the complete OData query string
   * @param includeQuestionMark Whether to include the leading '?' character
   * @returns The complete OData query string
   */
  build(includeQuestionMark: boolean = true): string {
    const parts: string[] = [];

    if (this.options.filter) {
      const filterString = this.buildFilterString(this.options.filter);
      if (filterString) {
        parts.push(`$filter=${encodeURIComponent(filterString)}`);
      }
    }

    if (this.options.orderBy && this.options.orderBy.length > 0) {
      const orderByString = this.buildOrderByString(this.options.orderBy);
      parts.push(`$orderby=${encodeURIComponent(orderByString)}`);
    }

    if (this.options.top !== undefined && this.options.top !== null) {
      parts.push(`$top=${this.options.top}`);
    }

    if (this.options.skip !== undefined && this.options.skip !== null) {
      parts.push(`$skip=${this.options.skip}`);
    }

    if (this.options.count !== undefined && this.options.count !== null) {
      parts.push(`$count=${this.options.count}`);
    }

    const queryString = parts.join('&');
    return queryString ? (includeQuestionMark ? '?' : '') + queryString : '';
  }

  /**
   * Builds the OData query string without URL encoding (for readability)
   * @param includeQuestionMark Whether to include the leading '?' character
   * @returns The complete OData query string without encoding
   */
  buildUnencoded(includeQuestionMark: boolean = true): string {
    const parts: string[] = [];

    if (this.options.filter) {
      const filterString = this.buildFilterString(this.options.filter);
      if (filterString) {
        parts.push(`$filter=${filterString}`);
      }
    }

    if (this.options.orderBy && this.options.orderBy.length > 0) {
      const orderByString = this.buildOrderByString(this.options.orderBy);
      parts.push(`$orderby=${orderByString}`);
    }

    if (this.options.top !== undefined && this.options.top !== null) {
      parts.push(`$top=${this.options.top}`);
    }

    if (this.options.skip !== undefined && this.options.skip !== null) {
      parts.push(`$skip=${this.options.skip}`);
    }

    if (this.options.count !== undefined && this.options.count !== null) {
      parts.push(`$count=${this.options.count}`);
    }

    const queryString = parts.join('&');
    return queryString ? (includeQuestionMark ? '?' : '') + queryString : '';
  }

  /**
   * Builds the filter string from a filter expression tree
   */
  private buildFilterString(expression: FilterExpression): string {
    switch (expression.type) {
      case 'binary':
        return this.buildBinaryExpression(expression as BinaryExpression);
      case 'unary':
        return this.buildUnaryExpression(expression as UnaryExpression);
      case 'function':
        return this.buildFunctionExpression(expression as FunctionExpression);
      case 'property':
        return this.buildPropertyExpression(expression as PropertyExpression);
      case 'literal':
        return this.buildLiteralExpression(expression as LiteralExpression);
      default:
        throw new Error(`Unknown expression type: ${(expression as any).type}`);
    }
  }

  /**
   * Builds a binary expression string
   */
  private buildBinaryExpression(expression: BinaryExpression): string {
    const left = this.buildFilterString(expression.left);
    const right = this.buildFilterString(expression.right);
    const operator = expression.operator;

    // Determine if we need parentheses based on operator precedence
    const needsParens = this.needsParentheses(expression);

    const result = `${left} ${operator} ${right}`;
    return needsParens ? `(${result})` : result;
  }

  /**
   * Builds a unary expression string
   */
  private buildUnaryExpression(expression: UnaryExpression): string {
    const operand = this.buildFilterString(expression.operand);
    const operator = expression.operator;

    if (operator === UnaryOperator.Negate) {
      return `-${operand}`;
    }

    return `${operator} ${operand}`;
  }

  /**
   * Builds a function expression string
   */
  private buildFunctionExpression(expression: FunctionExpression): string {
    const args = expression.arguments.map(arg => this.buildFilterString(arg));
    return `${expression.functionName}(${args.join(', ')})`;
  }

  /**
   * Builds a property expression string
   */
  private buildPropertyExpression(expression: PropertyExpression): string {
    return expression.propertyName;
  }

  /**
   * Builds a literal expression string
   */
  private buildLiteralExpression(expression: LiteralExpression): string {
    if (expression.value === null) {
      return 'null';
    }

    switch (expression.literalType) {
      case LiteralType.String:
        // Escape single quotes in strings
        const escapedString = String(expression.value).replace(/'/g, "''");
        return `'${escapedString}'`;

      case LiteralType.Boolean:
        return String(expression.value).toLowerCase();

      case LiteralType.Number:
        return String(expression.value);

      case LiteralType.DateTime:
        if (expression.value instanceof Date) {
          return expression.value.toISOString();
        }
        return String(expression.value);

      case LiteralType.Guid:
        return String(expression.value);

      case LiteralType.Null:
        return 'null';

      default:
        return String(expression.value);
    }
  }

  /**
   * Builds the orderBy string from orderBy clauses
   */
  private buildOrderByString(clauses: OrderByClause[]): string {
    return clauses.map(clause => {
      const direction = clause.direction === OrderDirection.Descending ? ' desc' : '';
      return `${clause.property}${direction}`;
    }).join(', ');
  }

  /**
   * Determines if a binary expression needs parentheses
   */
  private needsParentheses(expression: BinaryExpression): boolean {
    // Check if left or right operands are binary expressions with lower precedence
    const precedence = this.getOperatorPrecedence(expression.operator);

    if (expression.left.type === 'binary') {
      const leftPrecedence = this.getOperatorPrecedence((expression.left as BinaryExpression).operator);
      if (leftPrecedence < precedence) {
        return true;
      }
    }

    if (expression.right.type === 'binary') {
      const rightPrecedence = this.getOperatorPrecedence((expression.right as BinaryExpression).operator);
      if (rightPrecedence < precedence) {
        return true;
      }
    }

    return false;
  }

  /**
   * Gets the precedence level for an operator
   */
  private getOperatorPrecedence(operator: BinaryOperator): number {
    switch (operator) {
      case BinaryOperator.Or:
        return 1;
      case BinaryOperator.And:
        return 2;
      case BinaryOperator.Equal:
      case BinaryOperator.NotEqual:
      case BinaryOperator.GreaterThan:
      case BinaryOperator.GreaterThanOrEqual:
      case BinaryOperator.LessThan:
      case BinaryOperator.LessThanOrEqual:
        return 3;
      case BinaryOperator.Add:
      case BinaryOperator.Subtract:
        return 4;
      case BinaryOperator.Multiply:
      case BinaryOperator.Divide:
      case BinaryOperator.Modulo:
        return 5;
      default:
        return 0;
    }
  }

  /**
   * Static method to build a query string from options
   */
  static buildQuery(options: ODataQueryOptions, includeQuestionMark: boolean = false): string {
    return new ODataQueryBuilder(options).build(includeQuestionMark);
  }

  /**
   * Static method to build an unencoded query string from options
   */
  static buildQueryUnencoded(options: ODataQueryOptions, includeQuestionMark: boolean = false): string {
    return new ODataQueryBuilder(options).buildUnencoded(includeQuestionMark);
  }
}

/**
 * Helper functions for creating filter expressions
 */
export class FilterBuilder {
  /**
   * Creates a binary expression
   */
  static binary(left: FilterExpression, operator: BinaryOperator, right: FilterExpression): BinaryExpression {
    return { type: 'binary', left, operator, right };
  }

  /**
   * Creates a property expression
   */
  static property(name: string): PropertyExpression {
    return { type: 'property', propertyName: name };
  }

  /**
   * Creates a string literal expression
   */
  static string(value: string): LiteralExpression {
    return { type: 'literal', value, literalType: LiteralType.String };
  }

  /**
   * Creates a number literal expression
   */
  static number(value: number): LiteralExpression {
    return { type: 'literal', value, literalType: LiteralType.Number };
  }

  /**
   * Creates a boolean literal expression
   */
  static boolean(value: boolean): LiteralExpression {
    return { type: 'literal', value, literalType: LiteralType.Boolean };
  }

  /**
   * Creates a null literal expression
   */
  static null(): LiteralExpression {
    return { type: 'literal', value: null, literalType: LiteralType.Null };
  }

  /**
   * Creates a datetime literal expression
   */
  static datetime(value: Date): LiteralExpression {
    return { type: 'literal', value: new Date(value), literalType: LiteralType.DateTime };
  }

  /**
   * Creates a function expression
   */
  static function(name: string, ...args: FilterExpression[]): FunctionExpression {
    return { type: 'function', functionName: name, arguments: args };
  }

  /**
   * Creates a unary expression
   */
  static unary(operator: UnaryOperator, operand: FilterExpression): UnaryExpression {
    return { type: 'unary', operator, operand };
  }

  // Convenience methods for common operations
  static eq(left: FilterExpression, right: FilterExpression): BinaryExpression {
    return FilterBuilder.binary(left, BinaryOperator.Equal, right);
  }

  static ne(left: FilterExpression, right: FilterExpression): BinaryExpression {
    return FilterBuilder.binary(left, BinaryOperator.NotEqual, right);
  }

  static gt(left: FilterExpression, right: FilterExpression): BinaryExpression {
    return FilterBuilder.binary(left, BinaryOperator.GreaterThan, right);
  }

  static ge(left: FilterExpression, right: FilterExpression): BinaryExpression {
    return FilterBuilder.binary(left, BinaryOperator.GreaterThanOrEqual, right);
  }

  static lt(left: FilterExpression, right: FilterExpression): BinaryExpression {
    return FilterBuilder.binary(left, BinaryOperator.LessThan, right);
  }

  static le(left: FilterExpression, right: FilterExpression): BinaryExpression {
    return FilterBuilder.binary(left, BinaryOperator.LessThanOrEqual, right);
  }

  static and(left: FilterExpression, right: FilterExpression): BinaryExpression {
    return FilterBuilder.binary(left, BinaryOperator.And, right);
  }

  static or(left: FilterExpression, right: FilterExpression): BinaryExpression {
    return FilterBuilder.binary(left, BinaryOperator.Or, right);
  }

  static isAnyOf(propertyExpr: PropertyExpression, values: LiteralExpression[]): FilterExpression {
      let result: FilterExpression = <any>undefined;
      for (let i = 0; i < values.length; i++) {
          if (i == 0) {
              result = FilterBuilder.eq(propertyExpr, values[i]);
              continue;
          }
          result = FilterBuilder.or(result, FilterBuilder.eq(propertyExpr, values[i]));
      }
      return result;
  }

  static not(operand: FilterExpression): UnaryExpression {
    return FilterBuilder.unary(UnaryOperator.Not, operand);
  }

  static contains(property: string, value: string): FunctionExpression {
    return FilterBuilder.function('contains', FilterBuilder.property(property), FilterBuilder.string(value));
  }

  static startsWith(property: string, value: string): FunctionExpression {
    return FilterBuilder.function('startswith', FilterBuilder.property(property), FilterBuilder.string(value));
  }

  static endsWith(property: string, value: string): FunctionExpression {
    return FilterBuilder.function('endswith', FilterBuilder.property(property), FilterBuilder.string(value));
  }
}

/**
 * Helper class for building OrderBy clauses
 */
export class OrderByBuilder {
  /**
   * Creates an ascending order clause
   */
  static asc(property: string): OrderByClause {
    return { property, direction: OrderDirection.Ascending };
  }

  /**
   * Creates a descending order clause
   */
  static desc(property: string): OrderByClause {
    return { property, direction: OrderDirection.Descending };
  }
}
