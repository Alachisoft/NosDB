// /*
// * Copyright (c) 2016, Alachisoft. All Rights Reserved.
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// * http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// */
using System;

namespace Alachisoft.NosDB.Common.DataStructures
{
    /// <summary> Elements are added at the tail and removed from the head. Class is thread-safe in that
    /// 1 producer and 1 consumer may add/remove elements concurrently. The class is not
    /// explicitely designed for multiple producers or consumers. Implemented as a linked
    /// list, so that removal of an element at the head does not cause a right-shift of the
    /// remaining elements (as in a Vector-based implementation).
    /// </summary>
    /// <author>  Bela Ban
    /// </author>
    //	public class Queue
    //	{
    //		/// <summary> Returns the first element. Returns null if no elements are available.</summary>
    //		virtual public object First
    //		{
    //			get { return head != null?head.obj:null; }
    //		}
    //
    //		/// <summary> Returns the last element. Returns null if no elements are available.</summary>
    //		virtual public object Last
    //		{
    //			get { return tail != null?tail.obj:null; }			
    //		}
    //
    //		/*head and the tail of the list so that we can easily add and remove objects*/
    //		internal Element head = null, tail = null;
    //		
    //		/*flag to determine the state of the queue*/
    //		internal bool closed = false;
    //		
    //		/*current size of the queue*/
    //		private int size = 0;
    //		
    //		/* Lock object for synchronization. Is notified when element is added */
    //		internal object mutex = new object();
    //		
    //		/// <summary>Lock object for syncing on removes. It is notified when an object is removed </summary>
    //		// Object  remove_mutex=new Object();
    //		
    //		/*the number of end markers that have been added*/
    //		internal int num_markers = 0;
    //		
    //		/// <summary> if the queue closes during the runtime
    //		/// an endMarker object is added to the end of the queue to indicate that
    //		/// the queue will close automatically when the end marker is encountered
    //		/// This allows for a "soft" close.
    //		/// </summary>
    //		/// <seealso cref="Queue#close">
    //		/// </seealso>
    //		private static readonly object endMarker = new object();
    //		
    //		
    //		/// <summary> the class Element indicates an object in the queue.
    //		/// This element allows for the linked list algorithm by always holding a
    //		/// reference to the next element in the list.
    //		/// if Element.next is null, then this element is the tail of the list.
    //		/// </summary>
    //		internal class Element
    //		{
    //			/*the actual value stored in the queue*/
    //			internal object obj = null;
    //			/*pointer to the next item in the (queue) linked list*/
    //			internal Element next = null;
    //			
    //			/// <summary> creates an Element object holding its value</summary>
    //			/// <param name="o">- the object to be stored in the queue position
    //			/// </param>
    //			internal Element(object o)
    //			{
    //				//this.enclosingInstance = enclosingInstance;
    //				obj = o;
    //			}
    //			
    //			/// <summary> prints out the value of the object</summary>
    //			public override string ToString()
    //			{
    //				return obj != null?obj.ToString():"null";
    //			}
    //		}
    //		
    //		
    //		/// <summary> creates an empty queue</summary>
    //		public Queue()
    //		{
    //		}
    //		
    //		
    //		/// <summary> returns true if the Queue has been closed
    //		/// however, this method will return false if the queue has been closed
    //		/// using the close(true) method and the last element has yet not been received.
    //		/// </summary>
    //		/// <returns> true if the queue has been closed
    //		/// </returns>
    //		public bool Closed
    //		{
    //			get{return closed;}
    //		}
    //		
    //		/// <summary> adds an object to the tail of this queue
    //		/// If the queue has been closed with close(true) no exception will be
    //		/// thrown if the queue has not been flushed yet.
    //		/// </summary>
    //		/// <param name="obj">- the object to be added to the queue
    //		/// </param>
    //		/// <exception cref=""> QueueClosedException exception if closed() returns true
    //		/// </exception>
    //		public void  add(object obj)
    //		{
    //			if (obj == null)
    //			{
    ////				if (log.isErrorEnabled())
    ////					log.error("argument must not be null");
    //				return ;
    //			}
    //			if (closed)
    //				throw new QueueClosedException();
    //			if (this.num_markers > 0)
    //				throw new QueueClosedException("Queue.add(): queue has been closed. You can not add more elements. " + "Waiting for removal of remaining elements.");
    //			
    //			/*lock the queue from other threads*/
    //			lock (mutex)
    //			{
    //				/*create a new linked list element*/
    //				Element el = new Element(obj);
    //				/*check the first element*/
    //				if (head == null)
    //				{
    //					/*the object added is the first element*/
    //					/*set the head to be this object*/
    //					head = el;
    //					/*set the tail to be this object*/
    //					tail = head;
    //					/*set the size to be one, since the queue was empty*/
    //					size = 1;
    //				}
    //				else
    //				{
    //					/*add the object to the end of the linked list*/
    //					tail.next = el;
    //					/*set the tail to point to the last element*/
    //					tail = el;
    //					/*increase the size*/
    //					size++;
    //				}
    //				/*wake up all the threads that are waiting for the lock to be released*/
    //				Monitor.PulseAll(mutex);
    //			}
    //		}
    //		
    //		
    //		/// <summary> Adds a new object to the head of the queue
    //		/// basically (obj.equals(queue.remove(queue.add(obj)))) returns true
    //		/// If the queue has been closed with close(true) no exception will be
    //		/// thrown if the queue has not been flushed yet.
    //		/// </summary>
    //		/// <param name="obj">- the object to be added to the queue
    //		/// </param>
    //		/// <exception cref=""> QueueClosedException exception if closed() returns true
    //		/// 
    //		/// </exception>
    //		public void  addAtHead(object obj)
    //		{
    //			if (obj == null)
    //			{
    ////				if (log.isErrorEnabled())
    ////					log.error("argument must not be null");
    //				return ;
    //			}
    //			if (closed)
    //				throw new QueueClosedException();
    //			if (this.num_markers > 0)
    //				throw new QueueClosedException("Queue.addAtHead(): queue has been closed. You can not add more elements. " + "Waiting for removal of remaining elements.");
    //			
    //			/*lock the queue from other threads*/
    //			lock (mutex)
    //			{
    //				Element el = new Element(obj);
    //				/*check the head element in the list*/
    //				if (head == null)
    //				{
    //					/*this is the first object, we could have done add(obj) here*/
    //					head = el;
    //					tail = head;
    //					size = 1;
    //				}
    //				else
    //				{
    //					/*set the head element to be the child of this one*/
    //					el.next = head;
    //					/*set the head to point to the recently added object*/
    //					head = el;
    //					/*increase the size*/
    //					size++;
    //				}
    //				/*wake up all the threads that are waiting for the lock to be released*/
    //				Monitor.PulseAll(mutex);
    //			}
    //		}
    //		
    //		
    //		/// <summary> Removes 1 element from head or <B>blocks</B>
    //		/// until next element has been added or until queue has been closed
    //		/// </summary>
    //		/// <returns> the first element to be taken of the queue
    //		/// </returns>
    //		public object remove()
    //		{
    //			object retval = null;
    //			try
    //			{
    //				retval = remove(Timeout.Infinite);
    //			}
    //			catch (TimeoutException)
    //			{
    //			}
    //			return retval;
    //		}
    //		
    //		
    //		/// <summary> Removes 1 element from the head.
    //		/// If the queue is empty the operation will wait for timeout ms.
    //		/// if no object is added during the timeout time, a Timout exception is thrown
    //		/// </summary>
    //		/// <param name="timeout">- the number of milli seconds this operation will wait before it times out
    //		/// </param>
    //		/// <returns> the first object in the queue
    //		/// </returns>
    //		public object remove(long timeout)
    //		{
    //			object retval = null;
    //			
    //			/*lock the queue*/
    //			lock (mutex)
    //			{
    //				/*if the queue size is zero, we want to wait until a new object is added*/
    //				if (size == 0)
    //				{
    //					if (closed) throw new QueueClosedException();
    //
    //					if(!Monitor.Wait(mutex, (int)timeout))
    //					{
    //						throw new TimeoutException();
    //					}
    //				}
    //				/*we either timed out, or got notified by the add_mutex lock object*/
    //				
    //				/*check to see if the object closed*/
    //				if (closed) throw new QueueClosedException();
    //				
    //				/*get the next value*/
    //				retval = removeInternal();
    //			
    //				/*if we reached an end marker we are going to close the queue*/
    //				if (retval == endMarker)
    //				{
    //					close(false);
    //					throw new QueueClosedException();
    //				}
    //				/*at this point we actually did receive a value from the queue, return it*/
    //				return retval;
    //			}
    //		}
    //		
    //		
    //		/// <summary> removes a specific object from the queue.
    //		/// the object is matched up using the Object.equals method.
    //		/// </summary>
    //		/// <param name="obj">the actual object to be removed from the queue
    //		/// </param>
    //		public void  removeElement(object obj)
    //		{
    //			Element el, tmp_el;
    //			
    //			if (obj == null)
    //			{
    ////				if (log.isErrorEnabled())
    ////					log.error("argument must not be null");
    //				return ;
    //			}
    //			
    //			/*lock the queue*/
    //			lock (mutex)
    //			{
    //				el = head;
    //				
    //				/*the queue is empty*/
    //				if (el == null)
    //					return ;
    //				
    //				/*check to see if the head element is the one to be removed*/
    //				if (el.obj.Equals(obj))
    //				{
    //					/*the head element matched we will remove it*/
    //					head = el.next;
    //					el.next = null;
    //					/*check if we only had one object left
    //					*at this time the queue becomes empty
    //					*this will set the tail=head=null
    //					*/
    //					if (size == 1)
    //						tail = head; // null
    //					decrementSize();
    //					
    //					//                if(size == 0) {
    //					//                    synchronized(remove_mutex) {
    //					//                        remove_mutex.notifyAll();
    //					//                    }
    //					//                }
    //					return ;
    //				}
    //				
    //				/*look through the other elements*/
    //				while (el.next != null)
    //				{
    //					if (el.next.obj.Equals(obj))
    //					{
    //						tmp_el = el.next;
    //						if (tmp_el == tail)
    //						// if it is the last element, move tail one to the left (bela Sept 20 2002)
    //							tail = el;
    //						el.next = el.next.next; // point to the el past the next one. can be null.
    //						tmp_el.next = null;
    //						decrementSize();
    //						//                    if(size == 0) {
    //						//                        synchronized(remove_mutex) {
    //						//                            remove_mutex.notifyAll();
    //						//                        }
    //						//                    }
    //						break;
    //					}
    //					el = el.next;
    //				}
    //			}
    //		}
    //		
    //		
    //		/// <summary> returns the first object on the queue, without removing it.
    //		/// If the queue is empty this object blocks until the first queue object has
    //		/// been added
    //		/// </summary>
    //		/// <returns> the first object on the queue
    //		/// </returns>
    //		public object peek()
    //		{
    //			object retval = null;
    //			try
    //			{
    //				retval = peek(Timeout.Infinite);
    //			}
    //			catch (TimeoutException)
    //			{
    //			}
    //			return retval;
    //		}
    //		
    //		
    //		/// <summary> returns the first object on the queue, without removing it.
    //		/// If the queue is empty this object blocks until the first queue object has
    //		/// been added or the operation times out
    //		/// </summary>
    //		/// <param name="timeout">how long in milli seconds will this operation wait for an object to be added to the queue
    //		/// before it times out
    //		/// </param>
    //		/// <returns> the first object on the queue
    //		/// </returns>
    //		
    //		public object peek(long timeout)
    //		{
    //			object retval = null;
    //			
    //			lock (mutex)
    //			{
    //				if (size == 0)
    //				{
    //					if (closed) throw new QueueClosedException();
    //
    //					if(!Monitor.Wait(mutex, (int)timeout))
    //					{
    //						throw new TimeoutException();
    //					}
    //				}
    //
    //				if (closed) throw new QueueClosedException();
    //				retval = head != null ? head.obj:null;
    //				
    //				if (retval == endMarker)
    //				{
    //					close(false);
    //					throw new QueueClosedException();
    //				}
    //				return retval;
    //			}
    //		}
    //		
    //		
    //		/// <summary>Marks the queues as closed. When an <code>add</code> or <code>remove</code> operation is
    //		/// attempted on a closed queue, an exception is thrown.
    //		/// </summary>
    //		/// <param name="flush_entries">When true, a end-of-entries marker is added to the end of the queue.
    //		/// Entries may be added and removed, but when the end-of-entries marker
    //		/// is encountered, the queue is marked as closed. This allows to flush
    //		/// pending messages before closing the queue.
    //		/// </param>
    //		public void  close(bool flush_entries)
    //		{
    //			if (flush_entries)
    //			{
    //				try
    //				{
    //					add(endMarker); // add an end-of-entries marker to the end of the queue
    //					num_markers++;
    //				}
    //				catch (QueueClosedException ex)
    //				{
    //					Trace.error("Queue.close()", "exception=" + ex.Message);
    //				}
    //				return ;
    //			}
    //			
    //			lock (mutex)
    //			{
    //				closed = true;
    //				Monitor.PulseAll(mutex);
    //			}
    //			
    //			//        synchronized(remove_mutex) {
    //			//            remove_mutex.notifyAll();
    //			//        }
    //		}
    //		
    //		
    //		/// <summary> resets the queue.
    //		/// This operation removes all the objects in the queue and marks the queue open
    //		/// </summary>
    //		public void  reset()
    //		{
    //			num_markers = 0;
    //			if (!closed)
    //				close(false);
    //			
    //			lock (mutex)
    //			{
    //				size = 0;
    //				head = null;
    //				tail = null;
    //				closed = false;
    //				Monitor.PulseAll(mutex);
    //			}
    //			
    //			//        synchronized(remove_mutex) {
    //			//            remove_mutex.notifyAll();
    //			//        }
    //		}
    //		
    //		/// <summary>
    //		/// Number of Objects in the queue
    //		/// </summary>
    //		public int Count
    //		{
    //			get{return size - num_markers;}
    //		}
    //		
    //		/// <summary> prints the size of the queue</summary>
    //		public override string ToString()
    //		{
    //			return "Queue (" + Count + ") messages";
    //		}
    //		
    //		
    //		/// <summary> Removes the first element. Returns null if no elements in queue.
    //		/// Always called with add_mutex locked (we don't have to lock add_mutex ourselves)
    //		/// </summary>
    //		private object removeInternal()
    //		{
    //			Element retval;
    //			
    //			/*if the head is null, the queue is empty*/
    //			if (head == null)
    //				return null;
    //			
    //			retval = head; // head must be non-null now
    //			
    //			head = head.next;
    //			if (head == null)
    //				tail = null;
    //			
    //			decrementSize();
    //			//        if(size == 0) {
    //			//            synchronized(remove_mutex) {
    //			//                remove_mutex.notifyAll();
    //			//            }
    //			//        }
    //			
    //			if (head != null && head.obj == endMarker)
    //			{
    //				closed = true;
    //			}
    //			
    //			retval.next = null;
    //			return retval.obj;
    //		}
    //		
    //		
    //		void  decrementSize()
    //		{
    //			size--;
    //			if (size < 0)
    //				size = 0;
    //		}
    //		
    //	}
    //

    internal class QueueClosedException : Exception
    {
        /// <summary>
        /// Basic Exception
        /// </summary>
        public QueueClosedException() { }
        /// <summary>
        /// Exception with custom message
        /// </summary>
        /// <param name="msg">Message to display when exception is thrown</param>
        public QueueClosedException(String msg) : base(msg) { }

        /// <summary>
        /// Creates a String representation of the Exception
        /// </summary>
        /// <returns>A String representation of the Exception</returns>
        public String toString()
        {
            if (this.Message != null)
                return "QueueClosedException:" + this.Message;
            else
                return "QueueClosedException";
        }
    }
}