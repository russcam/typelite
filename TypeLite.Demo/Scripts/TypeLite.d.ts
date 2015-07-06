

 



/// <reference path="Enums.ts" />

declare module Eshop {

  /**
   * Customer information
   */
  interface Customer {

    /**
     * Customer name.
     */
    Name: string;

    /**
     * Email address.
     */
    Email: string;

    /**
     * Customer is VIP member or not.
     */
    VIP: boolean;

    /**
     * Customer's kind.
     */
    Kind: Eshop.CustomerKind;

    /**
     * Customer's orders.
     */
    Orders: Eshop.Order[];
  }

  /**
   * a order
   */
  interface Order {

    /**
     * products
     */
    Products: Eshop.Product[];

    /**
     * total price
     */
    TotalPrice: number;

    /**
     * created date
     */
    Created: Date;

    /**
     * shipped date
     */
    Shipped: Date;
  }

  /**
   * product
   */
  interface Product {

    /**
     * name
     */
    Name: string;

    /**
     * price
     */
    Price: number;

    /**
     * ID
     */
    ID: System.Guid;
  }
}
declare module System {

  /**
   * Represents a globally unique identifier (GUID).
   */
  interface Guid {
  }
}
declare module TypeLite.Demo.Models {

  /**
   * Shipping Service
   */
  interface IShippingService {

    /**
     * price
     */
    Price: number;
  }
}
declare module Library {

  /**
   * Book
   */
  interface Book {

    /**
     * name
     */
    Title: string;

    /**
     * pages of book
     */
    Pages: number;

    /**
     * genre
     */
    Genre: Library.Genre;
  }

  /**
   * book library
   */
  interface Library {

    /**
     * name
     */
    Name: string;

    /**
     * books in library
     */
    Books: Library.Book[];
  }
}


