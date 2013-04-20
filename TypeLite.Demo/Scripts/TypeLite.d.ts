﻿

 


module Eshop {
interface Customer {
  Name: string;
  Email: string;
  VIP: bool;
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
module System {
interface Guid {
}
}