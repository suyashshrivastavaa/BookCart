﻿using BookCart.Dto;
using BookCart.Interfaces;
using BookCart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookCart.DataAccess
{
    public class OrderDataAccessLayer : IOrderService
    {
        readonly BookDBContext _dbContext;
        public OrderDataAccessLayer(BookDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public void CreateOrder(int userId, OrdersDto orderDetails)
        {
            try
            {
                StringBuilder orderid = new StringBuilder();
                orderid.Append(CreateRandomNumber(3));
                orderid.Append('-');
                orderid.Append(CreateRandomNumber(6));

                CustomerOrders customerOrder = new CustomerOrders
                {
                    OrderId = orderid.ToString(),
                    UserId = userId,
                    DateCreated = DateTime.Now.Date,
                    CartTotal = orderDetails.CartTotal
                };
                _dbContext.CustomerOrders.Add(customerOrder);
                _dbContext.SaveChanges();

                foreach (CartItemDto order in orderDetails.OrderDetails)
                {
                    CustomerOrderDetails productDetails = new CustomerOrderDetails
                    {
                        OrderId = orderid.ToString(),
                        ProductId = order.Book.BookId,
                        Quantity = order.Quantity,
                        Price = order.Book.Price
                    };
                    _dbContext.CustomerOrderDetails.Add(productDetails);
                    _dbContext.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public List<OrdersDto> GetOrderList(int userId)
        {
            List<OrdersDto> userOrders = new List<OrdersDto>();
            List<string> userOrderId = new List<string>();

            userOrderId = _dbContext.CustomerOrders.Where(x => x.UserId == userId)
                .Select(x => x.OrderId).ToList();

            foreach (string orderid in userOrderId)
            {
                OrdersDto order = new OrdersDto
                {
                    OrderId = orderid,
                    CartTotal = _dbContext.CustomerOrders.FirstOrDefault(x => x.OrderId == orderid).CartTotal,
                    OrderDate = _dbContext.CustomerOrders.FirstOrDefault(x => x.OrderId == orderid).DateCreated
                };

                List<CustomerOrderDetails> orderDetail = _dbContext.CustomerOrderDetails.Where(x => x.OrderId == orderid).ToList();

                order.OrderDetails = new List<CartItemDto>();

                foreach (CustomerOrderDetails cutomerOrder in orderDetail)
                {
                    CartItemDto item = new CartItemDto();

                    Book book = new Book
                    {
                        BookId = cutomerOrder.ProductId,
                        Title = _dbContext.Book.FirstOrDefault(x => x.BookId == cutomerOrder.ProductId && cutomerOrder.OrderId == orderid).Title,
                        Price = _dbContext.CustomerOrderDetails.FirstOrDefault(
                        x => x.ProductId == cutomerOrder.ProductId && cutomerOrder.OrderId == orderid).Price,
                    };

                    item.Book = book;
                    item.Quantity = _dbContext.CustomerOrderDetails.FirstOrDefault(
                        x => x.ProductId == cutomerOrder.ProductId && x.OrderId == orderid).Quantity;


                    order.OrderDetails.Add(item);
                }
                userOrders.Add(order);
            }
            return userOrders.OrderByDescending(x=> x.OrderDate).ToList();
        }

        int CreateRandomNumber(int length)
        {
            Random rnd = new Random();
            return rnd.Next(Convert.ToInt32(Math.Pow(10, length - 1)), Convert.ToInt32(Math.Pow(10, length)));
        }
    }
}