import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';
import { MessageService } from '../_services/message.service';
import { PresenceService } from '../_services/presence.service';
import { take } from 'rxjs';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages?: Message[]; // ?means optional
  pagination?: Pagination;
  container = 'Unread';
  pageNumber = 1;
  pageSize =5;
  loading = false
  

  constructor(private messageService : MessageService, private presenceService: PresenceService) { }

  ngOnInit(): void {
    this.loadMessages();
  }
  loadMessages(){
    this.loading = true;
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container).subscribe({
      next: response =>{
        this.messages = response.result;
        this.pagination = response.pagination;
        this.loading = false;
      }
    })
  }

  deleteMessage(id: number){
    this.messageService.deleteMessage(id).subscribe({
      next: _=>this.messages?.splice(this.messages?.findIndex(m=>m.id ===id),1)
    })
    this.presenceService.unreadMessagesNum$.pipe(take(1)).subscribe({
      next: unreadNum=>this.presenceService.unreadMessagesNumSource.next(unreadNum-1)
    })
  }

  pageChanged(event: any){
    if(this.pageNumber !== event.page){      
      this.pageNumber = event.page; //parameter
      this.loadMessages();
    }
  }

  resetPageAndLoadMessages(){
    this.pageNumber = 1;
    this.loadMessages();
  }

}
