

 



/// <reference path="Enums.ts" />

declare module Eshop {
export interface Customer {
  Name: string;
  Email: string;
  VIP: boolean;
  Kind: Eshop.CustomerKind;
  Orders: Eshop.Order[];
}
export interface Order {
  Products: Eshop.Product[];
  TotalPrice: number;
  Created: Date;
}
export interface Product {
  Name: string;
  Price: number;
  ID: System.Guid;
}
}
declare module System {
export interface Guid {
}
}
declare module Library {
export interface Book {
  Title: string;
  Pages: number;
  Genre: Library.Genre;
}
export interface Library {
  Name: string;
  Books: Library.Book[];
}
}


