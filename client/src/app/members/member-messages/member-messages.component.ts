import { ChangeDetectionStrategy, Component, ElementRef, Input, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ReplaySubject, takeUntil } from 'rxjs';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @Input() username?: string;
  @ViewChild('scrollMe') scrollMe?: ElementRef;
  @ViewChild('messageForm') messageForm?: NgForm;
  @ViewChildren('messages') messages?: QueryList<any>;
  messageContent='';
  loading=false;
  private destroy$ = new ReplaySubject<boolean>(1);

  constructor(public messageService: MessageService) { }

  ngOnInit(): void {

  }
  ngAfterViewInit() {
    this.scrollToBottom();
    if(!this.messages) return;
    this.messages?.changes.pipe(takeUntil(this.destroy$)).subscribe(this.scrollToBottom);
  }
  
  scrollToBottom = () => {
    if(!this.scrollMe) return
    try {
      this.scrollMe.nativeElement.scrollTop = this.scrollMe.nativeElement.scrollHeight;
    } catch (err) {}
  }
  
  sendMessage(){
    if(!this.username) return;
    this.loading=true;
    this.messageService.SendMessage(this.username, this.messageContent).then(()=>{
      this.messageForm?.reset();
    }).finally(()=>this.loading = false);//if it is completed or error we will come here
  }
  ngOnDestroy(){
    this.destroy$.next(true);
    this.destroy$.complete(); 
  }

}
