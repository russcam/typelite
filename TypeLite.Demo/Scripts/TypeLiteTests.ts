/// <reference path="TypeLite.d.ts" />

class CustomerImp {
    Name: string;
    Email: string;
    VIP: boolean;
    Kind: Eshop.CustomerKind;
    Orders: Eshop.Order[];
};

var customerObj: Eshop.Customer = new CustomerImp();
customerObj.Kind = Eshop.CustomerKind.Individual; 