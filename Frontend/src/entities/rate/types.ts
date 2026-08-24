/** Дата в формате yyyy-MM-dd */
export type DateString = string;

export interface Rate {
  /** Дата начала действия ставки, yyyy-MM-dd */
  from: DateString;
  /** Часовая ставка в рублях */
  value: number;
}
