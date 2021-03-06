using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToyStore.Models.DBModels;
using ToyStore.Models.Abstracts;
using ToyStore.Models.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using ToyStore.Models.ControllerModels;

namespace ToyStore.Repository.Models
{
    public class ToyRepository
    {

        private DbContextClass _db;


        public ToyRepository(DbContextClass _db)
        {
            this._db = _db;
        }

        public ToyRepository()
        {
        }

        // public ToyRepository()
        // {

        // }

        /* #region Authentications */

        /// <summary>
        /// checks if a customer with the same username exists.
        /// then check if passwords match.
        /// returns true if successful.
        /// </summary>
        /// <param name="authModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        public bool Login(AuthModel authModel, out Customer customer)
        {
            customer = null;
            try
            {
                using (var db = new DbContextClass())
                {
                    customer = db.Customers
                    .Where(c => c.CustomerUName == authModel.Username)
                    .Include(c => c.FinishedOrders)
                    .ThenInclude(o => o.cart)
                    .ThenInclude(s => s.Item)
                    .Include(c => c.CurrentOrder)
                    .ThenInclude(o => o.cart)
                    .ThenInclude(s => s.Item)
                    .First();
                }
                if (customer.ComparePasswords(authModel.Password))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                PrintError(e);
                customer = null;
                return false;
            }
            customer = null;
            return false;
        }


        /// <summary>
        /// checks if a customer with the same username exists.
        /// then check if passwords match.
        /// returns true if successful.
        /// </summary>
        /// <param name="authModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        public bool LoginTest(AuthModel authModel, out Customer customer)
        {
            customer = null;
            try
            {
                using (var db = new DbContextClass())
                {
                    customer = _db.Customers
                    .Where(c => c.CustomerUName == authModel.Username)
                    .Include(c => c.FinishedOrders)
                    .ThenInclude(o => o.cart)
                    .ThenInclude(s => s.Item)
                    .Include(c => c.CurrentOrder)
                    .ThenInclude(o => o.cart)
                    .ThenInclude(s => s.Item)
                    .First();
                }
                if (customer.ComparePasswords(authModel.Password))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                PrintError(e);
                customer = null;
                return false;
            }
            customer = null;
            return false;
        }

        /// <summary>
        /// This method is not for returning to user. <br/>
        /// This is only for updating the user when validating with token. <br/>
        /// in case they purchased an order or changed it.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public Customer GetCustomerWithId(Guid customerId)
        {
            Customer c = null;
            using (var db = new DbContextClass())
            {
                c = db.Customers.Where(c => c.CustomerId == customerId)
                .Include(c => c.CurrentOrder.cart)
                .ThenInclude(s => s.Item)
                .Include(c => c.CurrentOrder.cart)
                // .ThenInclude(s => s.location)
                // .ThenInclude(l => l.LocationInventory.Where(s => !s.PurchasedInOrder))
                .Include(c => c.CurrentOrder)
                .ThenInclude(o => o.OrderLocation)
                .ThenInclude(l => l.LocationInventory.Where(s => !s.PurchasedInOrder))
                // .ThenInclude(s => s.location)
                // .ThenInclude(s => s.order)
                .Include(c => c.FinishedOrders)
                .ThenInclude(o => o.cart)
                .ThenInclude(s => s.Item)
                .Include(c => c.FinishedOrders)
                .ThenInclude(o => o.cart)
                .ThenInclude(s => s.location)
                .FirstOrDefault();

                // foreach (var item in c.CurrentOrder.cart)
                // {
                // var stack = new SellableStack();
                // var cloneinv = new List<SellableStack>();
                // foreach (var loopstack in item.location.LocationInventory)
                // {
                //     db.Entry(loopstack).Reference(stack => stack.Item).Load();
                //     System.Console.WriteLine(loopstack.Item);
                //     if (!loopstack.PurchasedInOrder)
                //     {
                //         cloneinv.Add(loopstack);
                //     }
                // }
                // // var cloneinv = new List<SellableStack>(item.location.LocationInventory.Where(s => !s.PurchasedInOrder));
                // if (item == null)
                // {
                //     System.Console.WriteLine("validation: item null");
                // }
                // if (item.location == null)
                // {
                //     System.Console.WriteLine("validation: location null");
                //     db.Entry(item).Reference(s => s.location).Load();
                //     db.Entry(item.location).Collection(s => s.LocationInventory).Load();
                // }
                // if (item.location.LocationInventory == null)
                // {
                //     System.Console.WriteLine("validation: inventory null");
                //     db.Entry(item.location).Collection(s => s.LocationInventory).Load();
                // }
                // item.location.LocationInventory = cloneinv.ToHashSet();
                // item.location.LocationInventory.ToList().ForEach(stack =>
                // {
                //     _db.Entry(stack).Reference(s => s.Item);
                // });
                // item.location.LocationInventory = item.location.LocationInventory.Where(s => !s.PurchasedInOrder).ToHashSet();
                // }
            }
            return c;
        }

        public List<SellableStack> GetLocationInventory(Guid locationId)
        {
            List<SellableStack> stacks = null;
            stacks = _db.SellableStacks.Where(s => !s.PurchasedInOrder)
            .Where(s => s.location.LocationId == locationId)
            .Include(s => s.Item)
            .ToList();
            return stacks;
        }


        /// <summary>
        /// check if username exists, if not, add a new customer 
        /// with the received information to the db. <br/>
        /// returns true if successful
        /// </summary>
        /// <param name="authModel"></param>
        /// <param name="customerOut"></param>
        /// <returns></returns>
        public bool Register(AuthModel authModel, out Customer customerOut)
        {
            customerOut = null;
            try
            {
                using (var db = new DbContextClass())
                {
                    db.Customers.Where(c => c.CustomerUName == authModel.Username).First();
                }
                return false;
            }
            catch (System.Exception)
            {
                using (var db = new DbContextClass())
                {
                    var c = new Customer()
                    {
                        CustomerUName = authModel.Username,
                        FirstName = authModel.FirstName,
                        LastName = authModel.LastName,
                    };
                    c.SetPass(authModel.Password);
                    try
                    {
                        db.Customers.Add(c);
                        db.SaveChanges();
                        customerOut = db.Customers.Where(cu => cu.CustomerId == c.CustomerId)
                        .Include(c => c.FinishedOrders)
                        .ThenInclude(o => o.cart)
                        .Include(c => c.CurrentOrder)
                        .ThenInclude(o => o.cart)
                        .ThenInclude(s => s.Item)
                        .First();
                        return true;
                    }
                    catch (System.Exception e2)
                    {
                        Console.WriteLine("error: " + e2.Message + "\n" + e2.StackTrace);
                        return false;
                    }
                }
            }
        }



        /// <summary>
        /// check if username exists, if not, add a new customer 
        /// with the received information to the db. <br/>
        /// returns true if successful
        /// </summary>
        /// <param name="authModel"></param>
        /// <param name="customerOut"></param>
        /// <returns></returns>
        public bool RegisterTest(AuthModel authModel, out Customer customerOut)
        {
            customerOut = null;
            try
            {
                using (var db = new DbContextClass())
                {
                    _db.Customers.Where(c => c.CustomerUName == authModel.Username).First();
                }
                return false;
            }
            catch (System.Exception)
            {
                using (var db = new DbContextClass())
                {
                    var c = new Customer()
                    {
                        CustomerUName = authModel.Username,
                        FirstName = authModel.FirstName,
                        LastName = authModel.LastName,
                    };
                    c.SetPass(authModel.Password);
                    try
                    {
                        _db.Customers.Add(c);
                        _db.SaveChanges();
                        customerOut = _db.Customers.Where(cu => cu.CustomerId == c.CustomerId)
                        .Include(c => c.FinishedOrders)
                        .ThenInclude(o => o.cart)
                        .Include(c => c.CurrentOrder)
                        .ThenInclude(o => o.cart)
                        .ThenInclude(s => s.Item)
                        .First();
                        return true;
                    }
                    catch (System.Exception e2)
                    {
                        Console.WriteLine("error: " + e2.Message + "\n" + e2.StackTrace);
                        return false;
                    }
                }
            }
        }

        /* #endregion */

        /// <summary>
        /// Add a new tag to the db. <br/>
        /// returns true if success
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool AddTag(string name)
        {
            try
            {
                using (var db = new DbContextClass())
                {
                    db.Tags.Where(t => t.TagName == name).First();
                }
                return false;
            }
            catch (System.Exception)
            {
                try
                {
                    using (var db = new DbContextClass())
                    {
                        db.Tags.Add(new Tag() { TagName = name });
                        return db.SaveChanges() > 0;
                    }
                }
                catch (System.Exception e)
                {
                    PrintError(e);
                    return false;
                }
            }
        }



        /* #region Order Management */

        /// <summary>
        /// Add a new order to the DB. <br/>
        /// returns true if success.
        /// </summary>
        /// <param name="order"></param>
        /// <returns>true if success</returns>
        public bool SaveNewOrder(Order order)
        {
            using (var db = new DbContextClass())
            {
                db.Orders.Add(order);
                return db.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// Save all current customer changes. <br/>
        /// returns true if success.
        /// </summary>
        /// <param name="customer"></param>
        /// <returns>true if success</returns>
        public bool SaveCustomerChanges(Customer customer)
        {
            // using (var db = new DbContextClass())
            {
                if (!_db.Orders.Contains(customer.CurrentOrder))
                {
                    SaveNewOrder(customer.CurrentOrder);
                }
                else
                {
                    // var newCart = customer.CurrentOrder.cart;
                    var newOrder = customer.CurrentOrder;
                    var oldOrder = _db.Orders.Where(o => o.OrderId == newOrder.OrderId).FirstOrDefault();
                    foreach (var oldStack in oldOrder.cart)
                    {
                        if (!newOrder.cart.Contains(oldStack))
                        {
                            // removeStackFromOrder(oldOrder, oldStack);
                        }
                    }
                    foreach (var newStack in newOrder.cart)
                    {
                        // if (!_db.Sellables.Contains(pair.Key))
                        // {
                        //     // _db.Add(pair.)
                        // }
                        // if () { }
                    }
                    // customer.CurrentOrder.cart.ForEach(pizza =>
                    // {
                    // if (!_db.Pizzas.Contains(pizza))
                    // {
                    //     _db.Pizzas.Add(pizza);
                    // }
                    // else
                    // {
                    //     pizza.ToppingList.ForEach(topping =>
                    //     {
                    //         if (!_db.Toppings.Contains(topping))
                    //         {
                    //             _db.Toppings.Add(topping);
                    //         }
                    //     });
                    //     if (!_db.Crusts.Contains(pizza.PizzaCrust))
                    //     {
                    //         _db.Crusts.Add(pizza.PizzaCrust);
                    //     }
                    //     if (!_db.Sizes.Contains(pizza.PizzaSize))
                    //     {
                    //         _db.Sizes.Add(pizza.PizzaSize);
                    //     }
                    // }
                    // });
                    _db.SaveChanges();
                }
                try
                {
                    _db.Update(customer.CurrentOrder);
                    _db.Update(customer);
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
                {
                    Console.WriteLine("an error occurred while saving your changes: " + e.Message + "\n" + e.StackTrace);
                }
                catch (System.InvalidOperationException e)
                {
                    Console.WriteLine("an error occurred while saving your changes: " + e.Message + "\n" + e.StackTrace);
                }
                return _db.SaveChanges() > 0;
                // }
            }
        }


        /// <summary>
        /// Removes a sellable item from an order.
        /// adds the count back to the store stack.
        /// The change is done in the database.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="sellable"></param>
        /// <returns>true if success</returns>
        public bool RemoveSellableStackFromOrder(Order order, Guid stackId)
        {
            var stack = _db.SellableStacks
            .Where(s => s.SellableStackId == stackId)
            .Include(s => s.location)
            .Include(s => s.Item)
            .FirstOrDefault();
            if (stack == null)
            {
                System.Console.WriteLine("stack is not found");
                return false;
            }
            var location = stack.location;
            var locationStack = _db.SellableStacks
                .Where(s => s.Item.SellableId == stack.Item.SellableId)
                .Where(s => s.location.LocationId == location.LocationId)
                .Where(s => s.PurchasedInOrder == false)
                .Include(s => s.location)
                .FirstOrDefault();

            // foreach (var item in locationStack)
            // {
            System.Console.WriteLine("checking stacks in location");
            System.Console.WriteLine(locationStack.PurchasedInOrder);
            // }
            // return false;
            // var temp = locationStack.Where(s => s.Item.SellableId == stack.Item.SellableId);
            // foreach (var item in temp)
            // {
            //     System.Console.WriteLine("checking stack with id");
            //     System.Console.WriteLine(item.SellableStackId);
            //     System.Console.WriteLine(item);
            // }
            // return false;
            var purchasedCount = stack.Count;
            locationStack.Count += purchasedCount;
            _db.SellableStacks.Remove(stack);
            return _db.SaveChanges() > 0;
        }


        /// <summary>
        /// just removes a stack from the database.
        /// </summary>
        /// <param name="stack"></param>
        /// <returns>true if success</returns>
        // private bool _removeStack(SellableStack stack)
        // {
        //     _db.SellableStacks.Remove(stack);
        //     return _db.SaveChanges() > 0;
        // }


        /// <summary>
        /// Adds a new stack to the customer order.<br/>
        /// makes sure customer has an order first
        /// then gets the stack from the store
        /// decrement its count by the purchased count
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="purchasedStack"></param>
        /// <returns></returns>
        public bool AddSellableStackToCustomerOrder(Customer customer, SellableStack purchasedStack)
        {
            var dbLocation = _db.Locations.Where(s => s.LocationId == purchasedStack.location.LocationId).FirstOrDefault();
            customer = _db.Customers.Where(c => c.CustomerId == customer.CustomerId)
            .Include(c => c.CurrentOrder)
            .ThenInclude(o => o.cart)
            .ThenInclude(s => s.Item)
            .FirstOrDefault();
            if (customer.CurrentOrder == null)
            {
                customer.CurrentOrder = new Order()
                {
                    OrderId = Guid.NewGuid(),
                    cart = new HashSet<SellableStack>(),
                };
            }
            else if (customer.CurrentOrder.cart == null)
            {
                customer.CurrentOrder.cart = new HashSet<SellableStack>();
            }
            if (!_db.Orders.Contains(customer.CurrentOrder))
            {
                var order = _createNewDBOrder(customer.CurrentOrder);
                if (order == null)
                {
                    return false;
                }
                order.OrderLocation = dbLocation;
                customer.CurrentOrder = order;
                _db.SaveChanges();
            }
            customer.CurrentOrder.OrderDate = DateTime.Now;
            var purchasedCount = 1;

            // get stack representing stock of the location to decrement its count
            SellableStack locationStack = GetSellableStack(purchasedStack.location.LocationId, purchasedStack.SellableStackId);

            System.Console.WriteLine(locationStack.Item);
            // System.Console.WriteLine(locationStack.SellableStackId);
            // return false;
            // System.Console.WriteLine(locationStack.Item.SellableName);
            if (locationStack.Count < purchasedCount)
            {
                System.Console.WriteLine("\tCustom Error: can't decrement stack count");
                System.Console.WriteLine("\t\tstore count: " + locationStack.Count + " Customer wants: " + locationStack.Count);
                return false;
            }
            locationStack.Count -= purchasedCount;
            if (_db.SaveChanges() <= 0)
            {
                return false;
            }
            var newPurchasedProduct = _db.Sellables
            .Where(s => s.SellableId == purchasedStack.Item.SellableId)
            .Include(s => s.CurrentStacks)
            .FirstOrDefault();
            var customerStack = new SellableStack()
            {
                Count = purchasedCount,
                Item = newPurchasedProduct,
                location = dbLocation,
                locationId = dbLocation.LocationId,
                order = customer.CurrentOrder,
                PurchasedInOrder = true,
            };
            newPurchasedProduct.CurrentStacks.Add(customerStack);
            // _db.SaveChanges();
            // _db.SellableStacks.Add(customerStack);
            // if (_db.SaveChanges() <= 0)
            // {
            //     return false;
            // }
            customer.CurrentOrder.cart.Add(customerStack);
            customer.CurrentOrder.OrderLocation = dbLocation;
            // purchasedStack.SellableStackId = Guid.NewGuid();
            // customer.CurrentOrder.cart.Add(customerStack);
            // _db.Update(purchasedStack);
            // _db.Update(customer);
            // _db.Update(customer.CurrentOrder);
            return _db.SaveChanges() > 0;
        }


        /// <summary>
        /// saves the new order in the database. <br/>
        /// </summary>
        /// <param name="order"></param>
        /// <returns>returns the new order from the database</returns>
        private Order _createNewDBOrder(Order order)
        {
            _db.Add(order);
            _db.SaveChanges();
            return _db.Orders.Where(o => o.OrderId == order.OrderId).FirstOrDefault();
        }


        /// <summary>
        /// compares the customer's current order in the database
        /// with that of new one that was sent here. <br/>
        /// it checks to see if the required amount is not more than
        /// or less than what the user can have
        /// max is store stock and min is 1. <br/>
        /// Then if everything is ok it updates the store stock
        /// and then updates the user order stock.
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="newOrder"></param>
        /// <param name="stackwNewCount"></param>
        /// <returns>true if success</returns>
        public bool ChangeSellablStackCountInCustomerOrder(Customer customer, Guid stackId, int newStackCount)
        {

            customer = _db.Customers.Where(c => c.CustomerId == customer.CustomerId)
            .Include(c => c.CurrentOrder)
            .ThenInclude(o => o.cart)
            .ThenInclude(s => s.Item)
            .FirstOrDefault();
            Order oldOrder = customer.CurrentOrder;
            var stackwNewCount = _db.SellableStacks.Where(s => s.SellableStackId == stackId)
            .Include(s => s.location)
            .Include(s => s.Item)
            .FirstOrDefault();
            // int newStackCount = stackwNewCount.Count;
            if (newStackCount > 0 && oldOrder != null)
            {
                // get the store stack
                SellableStack locationItemStack = GetAllSellableStackswItemId(stackwNewCount.location.LocationId, stackwNewCount.Item.SellableId).FirstOrDefault();
                // get the stack in current order
                SellableStack oldStack = oldOrder.cart.Where(s => s.SellableStackId == stackwNewCount.SellableStackId).FirstOrDefault();
                if (oldStack != null && locationItemStack != null)
                {
                    int oldStackCount = oldStack.Count;
                    int diff = oldStackCount - newStackCount;
                    int storeStackStock = locationItemStack.Count;
                    if (storeStackStock + diff >= 0)
                    {
                        oldStack.Count = newStackCount;
                        locationItemStack.Count += diff;
                        return _db.SaveChanges() > 0;
                    }
                }

            }
            return false;
        }


        /// <summary>
        /// Check out a customer <br/>
        /// remove the customer's current order.
        /// Add the order to customer's finished orders.
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="order"></param>
        /// <returns>true if successful</returns>
        public bool CheckoutCustomer(Guid customerId)
        {
            var customer = _db.Customers.Where(customer => customer.CustomerId == customerId)
            .Include(c => c.CurrentOrder)
            .ThenInclude(o => o.OrderLocation)
            .Include(c => c.CurrentOrder)
            .ThenInclude(o => o.cart)
            .Include(c => c.FinishedOrders)
            .FirstOrDefault();

            var order = customer.CurrentOrder;
            if (order.cart == null || order.cart.Count <= 0)
            {
                return false;
            }

            order.OrderDate = DateTime.Now;
            _db.Orders.Remove(customer.CurrentOrder);
            _db.Update(customer);
            if (_db.SaveChanges() < 0)
            {
                return false;
            }

            if (customer.FinishedOrders == null)
            {
                customer.FinishedOrders = new List<Order>();
            }
            _db.Add(order);
            customer.FinishedOrders.Add(order);
            _db.Update(customer);
            return _db.SaveChanges() > 0;


            // using (var db = new DbContextClass())
            // {
            //     customer.CurrentOrder = null;
            //     order.OrderDate = DateTime.Now;
            //     if (db.Orders.Contains(order))
            //     {
            //         db.Remove(order);
            //         db.Update(customer);
            //         // db.Database.ExecuteSqlRaw("UPDATE dbo.Customers SET CurrentOrderOrderId = null WHERE CustomerId = '" + customer.CustomerId + "'");
            //         db.SaveChanges();
            //         // Console.WriteLine("change? :" + );
            //     }
            //     db.Add(order);
            //     customer.FinishedOrders.Add(order);
            //     db.Update(customer);
            //     return db.SaveChanges() > 0;
            // }
        }

        /* #endregion */

        /// <summary>
        /// Returns a list of all stacks available at a location. <br/>
        /// Searches for stacks with the particular Item.
        /// by default doesn't return stacks that are in an order
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="anywhere"></param>
        /// <returns></returns>
        public List<SellableStack> GetAllSellableStackswItemId(Guid locationId, Guid sellableId, bool anywhere = false)
        {
            List<SellableStack> stackList = new List<SellableStack>();

            stackList = _db.SellableStacks
                .Where(stack => stack.location.LocationId == locationId)
                .Where(stack => anywhere || !stack.PurchasedInOrder)
                .Where(stack => stack.Item.SellableId == sellableId)
                .Include(stack => stack.Item)
                .ThenInclude(sel => sel.Tags)
                .Include(stack => stack.Item)
                .ThenInclude(sel => sel.Products)
                .ThenInclude(sel => sel.Tags)
                .Include(s => s.location)
                .Include(s => s.order)
                .ToList();
            return stackList;
        }


        /// <summary>
        /// Get the stack with specific id at specific location
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="stackId"></param>
        /// <param name="anywhere"></param>
        /// <returns></returns>
        public SellableStack GetSellableStack(Guid locationId, Guid stackId, bool anywhere = false)
        {
            // List<SellableStack> stackList = new List<SellableStack>();

            var stackList = _db.SellableStacks
                .Where(stack => stack.location.LocationId == locationId)
                .Where(stack => anywhere || !stack.PurchasedInOrder)
                .Where(stack => stack.SellableStackId == stackId)
                .Include(stack => stack.Item)
                .ThenInclude(sel => sel.Tags)
                .Include(stack => stack.Item)
                .ThenInclude(sel => sel.Products)
                .ThenInclude(sel => sel.Tags)
                .Include(s => s.location)
                .Include(s => s.order)
                .FirstOrDefault();
            return stackList;
        }


        /// <summary>
        /// Returns a list of all stacks available at a location. <br/>
        /// by default doesn't return stacks that are in an order
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="anywhere"></param>
        /// <returns></returns>
        public List<SellableStack> GetAllSellableStacksInLocationId(Guid locationId, bool anywhere = false)
        {
            List<SellableStack> stackList = new List<SellableStack>();

            var locationList = _db.SellableStacks
                .Where(stack => stack.location.LocationId == locationId)
                .Where(stack => anywhere || stack.order == null)
                .Include(stack => stack.Item)
                .ThenInclude(sel => sel.Tags)
                .Include(stack => stack.Item)
                .ThenInclude(sel => sel.Products)
                .ThenInclude(sel => sel.Tags)
                .Include(s => s.location)
                .Include(s => s.order)
                .ToList();
            return stackList;
        }

        // /// <summary>
        // /// Returns a list of all stacks available at a location. <br/>
        // /// by default doesn't return stacks that are in an order
        // /// </summary>
        // /// <param name="locationId"></param>
        // /// <param name="anywhere"></param>
        // /// <returns></returns>
        // public List<SellableStack> GetAllSellableStacksInLocationId(Guid locationId, bool anywhere = false)
        // {
        //     List<SellableStack> stackList = new List<SellableStack>();

        //     var locationList = _db.SellableStacks
        //         .Where(stack => stack.location.LocationId == locationId)
        //         .Where(stack => anywhere || stack.order == null)
        //         .Include(stack => stack.Item)
        //         .ThenInclude(sel => sel.Tags)
        //         .Include(stack => stack.Item)
        //         .ThenInclude(sel => sel.Products)
        //         .ThenInclude(sel => sel.Tags)
        //         .Include(s => s.location)
        //         .Include(s => s.order)
        //         .ToList();
        //     return stackList;
        // }

        public List<SellableStack> GetAllSellableStacks()
        {
            List<SellableStack> stackList = new List<SellableStack>();
            using (var db = new DbContextClass())
            {
                var locationList = db.Locations
                    .Include(l => l.LocationInventory.Where(s => s.order == null))
                    .ThenInclude(stack => stack.Item)
                    .ThenInclude(st => st.Tags)
                    .Include(l => l.LocationInventory)
                    .ThenInclude(stack => stack.Item)
                    .ThenInclude(s => s.Products)
                    // .Include(l => l.LocationInventory)
                    // .ThenInclude(stack => stack.Item)
                    // .ThenInclude(s => s.CurrentOffer)
                    .ToList();
                // locationList.AddRange(
                //     db.Locations.Include(l => l.LocationInventory)
                //     .ThenInclude(stack => stack.Item)
                //     .ThenInclude(s => (s as Offer).Products)
                //     .ToList()
                // );
                locationList.ForEach(location =>
                {
                    if (location.LocationInventory != null && location.LocationInventory.Count > 0)
                    {
                        location.LocationInventory.ToList().ForEach(stack =>
                        {
                            System.Console.WriteLine("test: " + stack.Item);

                            // Console.WriteLine("test: " + stack.Item.SellableName + " " + stack.Item.Products[0].SellableName);

                            // if (stack.Item.CurrentOffer != null)
                            // {
                            //     Console.WriteLine("test: offer name: " + stack.Item.CurrentOffer.SellableName);
                            //     var o = stack.Item.CurrentOffer;
                            //     if (!o.Products.Contains(stack.Item))
                            //     {
                            //         o.Products.Add(stack.Item);
                            //         System.Console.WriteLine("test: product added: " + o.Products[0]);
                            //     }
                            // }
                            if (stack.order == null)
                                stackList.Add(stack);
                            // db.SellableStacks
                            // .Where(stack => stack.SellableStackId == stack.SellableStackId)
                            // .Include(st => st.Item)
                            // .ThenInclude(s => (s as Offer).Products);

                        });

                        // Console.WriteLine("test" + "not null");
                        // stackList.AddRange(location.LocationInventory);
                    }
                });
            }
            stackList.ForEach(stack =>
            {
                System.Console.WriteLine("sellable :" + stack.Item + " has count: " + stack.Count);
                if (stack.Item.Products != null && stack.Item.Products.Count > 0)
                {
                    System.Console.WriteLine("sellable :" + stack.Item + " has products");
                    stack.Item.Products.ForEach(sellable =>
                    {
                        System.Console.WriteLine("test: stack: " + sellable);
                    });
                }
            });
            return stackList;
            // return new List<SellableStack>()
            // {
            //     new SellableStack()
            //     {
            //         Item  = new Product()
            //         {
            //             SellableName = "Toy1",
            //             SellablePrice = 20,
            //             SellableImagePath = @"https://media.istockphoto.com/photos/brown-teddy-bear-isolated-in-front-of-a-white-background-picture-id909772478?k=6&m=909772478&s=612x612&w=0&h=X55jzpsKboa_jUjbEN8eqAn0gjt696ldbeJMEqmNrcU=",
            //             TagList = new List<Tag>()
            //             {
            //                 new Tag()
            //                 {
            //                     TagName = "Magnet"
            //                 }
            //             }
            //         },
            //         Count = 2
            //     },
            //     new SellableStack()
            //     {
            //         Item  = new Product()
            //         {
            //             SellableName = "Toy2",
            //             SellablePrice = 18,
            //             SellableImagePath = @"https://assets.ajio.com/medias/sys_master/root/h73/h6b/15562768613406/-473Wx593H-4914911360-multi-MODEL.jpg",
            //             TagList = new List<Tag>()
            //             {
            //                 new Tag()
            //                 {
            //                     TagName = "Doll"
            //                 }
            //             }
            //         },
            //         Count = 4
            //     },
            //     new SellableStack()
            //     {
            //         Item  = new Offer()
            //         {
            //             SellableName = "toy1+toy2",
            //             SellablePrice = 30,
            //             SellableImagePath = @"https://media.istockphoto.com/photos/brown-teddy-bear-isolated-in-front-of-a-white-background-picture-id909772478?k=6&m=909772478&s=612x612&w=0&h=X55jzpsKboa_jUjbEN8eqAn0gjt696ldbeJMEqmNrcU=",
            //             Products = new List<Product>()
            //             {
            //                 new Product()
            //                 {
            //                     SellableName = "Toy1",
            //                     SellablePrice = 20,
            //                     SellableImagePath = @"https://media.istockphoto.com/photos/brown-teddy-bear-isolated-in-front-of-a-white-background-picture-id909772478?k=6&m=909772478&s=612x612&w=0&h=X55jzpsKboa_jUjbEN8eqAn0gjt696ldbeJMEqmNrcU=",
            //                     TagList = new List<Tag>()
            //                     {
            //                         new Tag()
            //                         {
            //                             TagName = "Magnet"
            //                         }
            //                     }
            //                 },
            //                 new Product()
            //                 {
            //                     SellableName = "Toy2",
            //                     SellablePrice = 18,
            //                     SellableImagePath = @"https://media.istockphoto.com/photos/brown-teddy-bear-isolated-in-front-of-a-white-background-picture-id909772478?k=6&m=909772478&s=612x612&w=0&h=X55jzpsKboa_jUjbEN8eqAn0gjt696ldbeJMEqmNrcU=",
            //                     TagList = new List<Tag>()
            //                     {
            //                         new Tag()
            //                         {
            //                             TagName = "Doll"
            //                         }
            //                     }
            //                 }
            //             },
            //             TagList = new List<Tag>()
            //             {
            //                 new Tag()
            //                 {
            //                     TagName = "Doll"
            //                 },
            //                 new Tag()
            //                 {
            //                     TagName = "Magnet"
            //                 }
            //             },
            //         },
            //         Count = 4,
            //     }
            // };
        }

        /// <summary>
        /// get the first stack with this sellable id in it
        /// </summary>
        /// <param name="sellableId"></param>
        /// <returns></returns>
        public SellableStack GetStackwithSellableId(Guid sellableId, bool instore = true)
        {
            SellableStack stack = null;
            using (var db = new DbContextClass())
            {
                stack = db.SellableStacks
                .Where(s => s.Item.SellableId == sellableId)
                .Where(s => !instore || s.order == null)
                .Include(stack => stack.Item)
                .ThenInclude(sel => sel.Tags)
                .Include(stack => stack.Item)
                .ThenInclude(sel => sel.Products)
                .ThenInclude(sel => sel.Tags)
                .Include(s => s.location)
                .Include(s => s.order)
                .FirstOrDefault();

            }
            return stack;
        }


        /// <summary>
        /// Get the customers that bought this product id or sellable id
        /// </summary>
        /// <param name="sellableId"></param>
        /// <returns></returns>
        public List<Customer> GetCustomersWhoBoughtStack(Guid sellableId)
        {
            List<Customer> customers = new List<Customer>();
            // customers = _db.Customers.Where(c => c.FinishedOrders.Any(o => o.cart.Any(s => s.Item.SellableId == sellableId))).ToList();
            // return customers;

            using (var db = new DbContextClass())
            {
                var stacks = _db.SellableStacks.Where(s => s.PurchasedInOrder)
                .Include(s => s.order)
                .ThenInclude(o => o.OrderedBy)
                .Include(s => s.Item)
                .ToList();

                stacks.ForEach(s =>
                {
                    var customer = s.order.OrderedBy;
                    if (customer != null && !customers.Contains(customer))
                    {
                        customers.Add(customer);
                    }
                });
                return customers;

                // customers = db.Customers.Where(c => c.FinishedOrders.Any(o => o.cart.Any(s => s.Item.SellableId == sellableId)))
                // .Include(c => c.FinishedOrders.Where(o => o.cart.Any(s => s.Item.SellableId == sellableId)))
                // .ThenInclude(o => o.cart.Where(s => s.Item.SellableId == sellableId))
                // .ToList();
                // customers.ForEach(c =>
                // {
                //     System.Console.WriteLine("c.FirstName");
                //     System.Console.WriteLine(c.FirstName);
                //     c.FinishedOrders.ForEach(Order =>
                //     {
                //         Order.cart.ToList().ForEach(stack =>
                //         {
                //             System.Console.WriteLine("customers of a stack...");
                //             System.Console.WriteLine(c.FirstName);
                //             System.Console.WriteLine(stack.Item.SellableName);
                //         });
                //     });
                // });
            }
            return customers;
        }


        /// <summary>
        /// Get the stack with the exact id. <br/>
        /// but makes sure by default that it is not in any store.
        /// </summary>
        /// <param name="stackId"></param>
        /// <returns></returns>
        public SellableStack GetSellableStackByIdDb(Guid stackId, bool instore = true)
        {
            SellableStack stack = null;
            using (var db = new DbContextClass())
            {
                try
                {
                    stack = db.SellableStacks
                    .Where(s => s.SellableStackId == stackId)
                    .Where(s => !instore || s.order == null)
                    .Include(stack => stack.Item)
                    .ThenInclude(sel => sel.Tags)
                    .Include(stack => stack.Item)
                    .ThenInclude(sel => sel.Products)
                    .ThenInclude(sel => sel.Tags)
                    .Include(s => s.location)
                    .Include(s => s.order)
                    .First();
                    // db.Entry(stack.Item).Collection(s => s.Products).Load();
                }
                catch (System.Exception e)
                {
                    PrintError(e);
                }
            }
            return stack;
        }


        /// <summary>
        /// This method returns all the stacks in the database. <br/>
        /// and by default returns ones that are not in a customer order.
        /// </summary>
        /// <param name="sellableId"></param>
        /// <param name="instore"></param>
        /// <returns></returns>
        public List<SellableStack> GetSellableStacksWithItem(Guid sellableId, bool instore = true)
        {
            List<SellableStack> sellableStacks = null;
            using (var db = new DbContextClass())
            {
                sellableStacks = db.SellableStacks
                .Where(stack => stack.Item.SellableId == sellableId)
                .Where(s => !instore || s.order == null)
                .Include(stack => stack.Item)
                .ThenInclude(sel => sel.Tags)
                .Include(stack => stack.Item)
                .ThenInclude(sel => sel.Products)
                .ThenInclude(sel => sel.Tags)
                .Include(s => s.location)
                .Include(s => s.order)
                .ToList();
            }
            return sellableStacks;
        }

        public HashSet<Tag> GetAvailableTags()
        {
            var stackList = new List<SellableStack>();
            var tagSet = new HashSet<Tag>();
            using (var db = new DbContextClass())
            {
                stackList = GetAllSellableStacks();
            }
            if (stackList != null)
            {
                stackList.ForEach(stack =>
                {
                    var ts = stack.GetTags();
                    if (ts != null)
                    {
                        ts.ForEach(tag =>
                        {
                            tagSet.Add(tag);
                        });
                    }
                });
            }
            return tagSet;
        }

        public List<SellableStack> GetSellableStacksByTag(Tag neededTag)
        {
            var tagSellables = new List<SellableStack>();
            var allSellableStacks = GetAllSellableStacks();
            if (allSellableStacks != null && allSellableStacks.Count != 0)
            {
                tagSellables = allSellableStacks.Where(stack => stack.GetTags().Any(t => t.TagName == neededTag.TagName)).ToList();
            }
            else
            {
                Console.WriteLine("no items in db");
            }
            return tagSellables;
        }

        public List<SellableStack> GetRecommendedTags(Guid customerId)
        {
            List<SellableStack> stacks = new List<SellableStack>();
            // var customer = _db.Customers.Where(c => c.CustomerId == customerId)
            // .Include(c => c.FinishedOrders).ThenInclude(o => o.cart).ThenInclude(s => s.Item.Tags)
            // .ToList();
            var customerTags = _db.Tags.Include(t => t.TagSellables)
            .ThenInclude(t => t.CurrentStacks).ThenInclude(s => s.order.OrderedBy)
            .ToList();


            customerTags.ForEach(tag =>
            {
                foreach (var sellable in tag.TagSellables)
                {
                    foreach (var stack in sellable.CurrentStacks)
                    {
                        if (stack == null)
                        {
                            System.Console.WriteLine("stack is null");
                            continue;
                        }
                        if (stack.order != null && stack.order.OrderedBy != null && stack.order.OrderedBy.CustomerId == customerId && !stacks.Contains(stack))
                        {
                            stacks.Add(stack);
                        }
                    }
                }
            });

            return stacks;
        }

        private void PrintError(Exception e)
        {
            Console.WriteLine("error: " + e.Message + "\n" + e.StackTrace);
        }

        public void SeedDB()
        {
            var t1 = new Tag()
            {
                TagName = "FluffyDoll",
                TagSellables = new List<Sellable>()
            };
            var t2 = new Tag()
            {
                TagName = "Doll",
                TagSellables = new List<Sellable>()
            };
            var t3 = new Tag()
            {
                TagName = "Magnetic",
                TagSellables = new List<Sellable>()
            };
            var p1 = new Product()
            {
                SellableId = Guid.NewGuid(),
                SellableName = "TeddyBear",
                SellablePrice = 20,
                SellableDescription = "A Sweet Lovely Teddy Bear",
                SellableImagePath = @"https://media.istockphoto.com/photos/brown-teddy-bear-isolated-in-front-of-a-white-background-picture-id909772478?k=6&m=909772478&s=612x612&w=0&h=X55jzpsKboa_jUjbEN8eqAn0gjt696ldbeJMEqmNrcU=",
            };
            var p2 = new Product()
            {
                SellableId = Guid.NewGuid(),
                SellableName = "Barbey Doll",
                SellablePrice = 18,
                SellableDescription = "A Sweet Barbey Doll",
                SellableImagePath = @"https://assets.ajio.com/medias/sys_master/root/h73/h6b/15562768613406/-473Wx593H-4914911360-multi-MODEL.jpg",
            };
            var p3 = new Product()
            {
                SellableId = Guid.NewGuid(),
                SellableName = "Magnetic Game",
                SellablePrice = 25,
                SellableDescription = "A Creative tool for your child to experiment with",
                SellableImagePath = @"https://upload.wikimedia.org/wikipedia/commons/thumb/7/70/M_tic.jpg/220px-M_tic.jpg",
            };
            // var st1_1 = new SellableTag()
            // {
            //     SellableItem = p1,
            //     TagType = t1,
            // };
            // var st1_2 = new SellableTag()
            // {
            //     SellableItem = p1,
            //     TagType = t2,
            // };
            // p1.SellableTags = new List<SellableTag>() { st1_1, st1_2 };
            p1.Tags = new List<Tag>() { t1, t2 };
            t1.TagSellables.Add(p1);
            t2.TagSellables.Add(p1);
            // var st2_2 = new SellableTag()
            // {
            //     SellableItem = p2,
            //     TagType = t2,
            // };
            p2.Tags = new List<Tag>() { t2 };
            t2.TagSellables.Add(p2);
            // var st3_3 = new SellableTag()
            // {
            //     SellableItem = p3,
            //     TagType = t3,
            // };
            p3.Tags = new List<Tag>() { t3 };
            t3.TagSellables.Add(p3);

            var s1 = new SellableStack()
            {
                SellableStackId = Guid.NewGuid(),
                Item = p1,
                Count = 2
            };
            var s2 = new SellableStack()
            {
                SellableStackId = Guid.NewGuid(),
                Item = p2,
                Count = 4
            };
            var s4 = new SellableStack()
            {
                SellableStackId = Guid.NewGuid(),
                Item = p3,
                Count = 9
            };
            var o1 = new Offer()
            {
                SellableId = Guid.NewGuid(),
                SellableName = "toy1+toy2",
                SellablePrice = 30,
                SellableImagePath = @"https://media.istockphoto.com/photos/brown-teddy-bear-isolated-in-front-of-a-white-background-picture-id909772478?k=6&m=909772478&s=612x612&w=0&h=X55jzpsKboa_jUjbEN8eqAn0gjt696ldbeJMEqmNrcU=",
                Products = new List<Sellable>()
                {
                    p1,
                    p2
                },
            };
            // var ot1_1 = new SellableTag()
            // {
            //     SellableItem = o1,
            //     TagType = t1,
            // };
            // var ot1_2 = new SellableTag()
            // {
            //     SellableItem = o1,
            //     TagType = t2,
            // };
            o1.Tags = new List<Tag>() { t1, t2 };
            t1.TagSellables.Add(o1);
            t2.TagSellables.Add(o1);


            var s3 = new SellableStack()
            {
                SellableStackId = Guid.NewGuid(),
                Item = o1,
                Count = 5,
            };
            using (var db = new DbContextClass())
            {
                db.SellableStacks.Add(s1);
                db.SellableStacks.Add(s2);
                db.SellableStacks.Add(s3);
                db.SellableStacks.Add(s4);
                db.SaveChanges();
            }
            var stacks = new List<SellableStack>();
            using (var db = new DbContextClass())
            {
                stacks = db.SellableStacks.ToList();
            }

            var l1 = new Location()
            {
                LocationId = Guid.NewGuid(),
                LocationName = "NewYork Store",
                // LocationInventory = new HashSet<SellableStack>() { stacks[3], stacks[1] }
            };
            var l2 = new Location()
            {
                LocationId = Guid.NewGuid(),
                LocationName = "Chicago Store",
                // LocationInventory = new HashSet<SellableStack>() { stacks[2], stacks[0] }
            };
            using (var db = new DbContextClass())
            {
                db.Locations.Add(l1);
                db.Locations.Add(l2);
                db.SaveChanges();
            }

            using (var db = new DbContextClass())
            {
                var stores = db.Locations.ToList();
                stores[0].LocationInventory = new HashSet<SellableStack>();
                stores[0].LocationInventory.Add(stacks[0]);
                stores[0].LocationInventory.Add(stacks[2]);
                db.SaveChanges();
            }

            using (var db = new DbContextClass())
            {
                var stores = db.Locations.ToList();
                stores[1].LocationInventory = new HashSet<SellableStack>();
                stores[1].LocationInventory.Add(stacks[1]);
                stores[1].LocationInventory.Add(stacks[3]);
                db.SaveChanges();
            }
        }


        /* #endregion */

    }
}
