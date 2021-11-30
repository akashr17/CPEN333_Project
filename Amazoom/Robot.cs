﻿using System;
using System.Windows;
using System.Collections.Generic;

namespace Amazoom
{
    public class Robot
    {
        private int id;
        private double currBatteryLevel;
        private double maxLoadingCap = 100.0;
        private double currentLoad = 0.0;
        private int[] location;
        private bool isActive = false;
        private Queue<(Item, Shelf)> robotQueue = new Queue<(Item, Shelf)>();



        public Robot(int id, int[] location)
        {
            this.id = id;
            this.currBatteryLevel = 100;
            this.location = location;
        }

        /*
         * @param: Tuple<Item,Shelf>
         * @return: void
         * queues items to be processed from an Order for current robot
         * */
        public void QueueItem((Item, Shelf) item, int quantity)
        {
            for(int i = 0; i < quantity; i++)
            {
                this.robotQueue.Enqueue(item);
            }
            return;
        }

        /*
         * @return: void
         * move robot to item's location in warehouse and retrieve item. Decrement inventory
         * */
        public void getOrder(Order order)
        {
            Item[] inventory = Computer.ReadInventory();
            for(int i=0; i < order.items.Count; i++)
            {
                order.items[i] = (order.items[i].item, 0); //the quantity is reset to 0 for every item in that order
            }

            //process all items of current order in queue
            while(this.robotQueue.Count > 0)
            {
                (Item, Shelf) currItem = this.robotQueue.Dequeue();
                Shelf currShelf = currItem.Item2;
                if(this.currentLoad + currItem.Item1.weight <= this.maxLoadingCap)
                {
                    this.location = currShelf.shelfLocation.location; //location of a specific item within our warehouse grid
                    for (int i = 0; i < currShelf.items.Count; i++) //iterate over items in that shelf and remove item being processed
                    {
                        if (currShelf.items[i].id == currItem.Item1.id)
                        {
                            currShelf.items.RemoveAt(i);
                            currShelf.currWeight -= currItem.Item1.weight;
                            //** decrement inventory as well for removed item **

                            inventory[currItem.Item1.id].stock -= 1;
                        }
                    }
                    this.currentLoad += currItem.Item1.weight;
                    for (int i = 0; i < order.items.Count; i++)
                    {
                        if(order.items[i].item.id == currItem.Item1.id)
                        {
                            order.items[i] = (order.items[i].item, order.items[i].quantity+1); //the quantity is incremented for every item retrieved from warehouse
                            break;
                        }
                        
                    }

                }
                else
                {
                    //*****move robot to dock, drop stuff off at bin, come back for remaining items
                    this.location = new int[2] {0,0}; //this location should be wherever we de-load our items if robot capacity is full
                    this.currentLoad = 0.0; //reset load

                }
            }
            //order completed, queue item for delivery
            Computer.processedOrders.Enqueue(order);
            Computer.UpdateInventory(inventory);
            return;
        }

        //setters and getters
        public void setActiveStatus(bool isActive)
        {
            this.isActive = isActive;
        }

        public bool getActiveStatus()
        {
            return this.isActive;
        }

        public void setLocation(int[] location)
        {
            this.location = location;
        }

        public int[] getLocation()
        {
            return this.location;
        }

        public void setBatteryLevel(double level)
        {
            this.currBatteryLevel = level;
        }

        public double getBatteryLevel()
        {
            return this.currBatteryLevel;
        }

    }
}
