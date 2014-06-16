

 



/// <reference path="Enums.ts" />

declare module Eshop {
interface Customer {
  Name: string;
  Email: string;
  VIP: boolean;
  Kind: Eshop.CustomerKind;
  Orders: Eshop.Order[];
}
interface Order {
  Products: Eshop.Product[];
  TotalPrice: number;
  Created: Date;
}
interface Product {
  Name: string;
  Price: number;
  ID: System.Guid;
}
}
declare module System {
interface Guid {
}
}
declare module Library {
interface Book {
  Title: string;
  Pages: number;
  Genre: Library.Genre;
}
interface Library {
  Name: string;
  Books: Library.Book[];
}
}


