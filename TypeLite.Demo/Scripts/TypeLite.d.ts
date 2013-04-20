

 


module Eshop {
interface Customer {
  Name: string;
  Email: string;
  VIP: bool;
  Orders: Order[];
}
interface Order {
  Products: Product[];
  TotalPrice: number;
  Created: Date;
}
interface Product {
  Name: string;
  Price: number;
}
}
