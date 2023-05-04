import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { BehaviorSubject,pipe,take } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection?:HubConnection//we are not gonna have it when we initially access this server => getting it from SignalR
  private onlineUsersSource = new BehaviorSubject<string[]>([]) //behaviorSubject can have an initial value here it is an empty string
  onlineUsers$ = this.onlineUsersSource.asObservable();
  public unreadMessagesNumSource = new BehaviorSubject<number>(0) //behaviorSubject can have an initial value here it is an empty string
  unreadMessagesNum$ = this.unreadMessagesNumSource.asObservable();
  constructor(private toastr: ToastrService, private router: Router) { }
// difference between observable and subject
//https://www.youtube.com/watch?v=Zr3kwMiAfRE

  createHubConnection(user: User){
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl +'presence',{
        accessTokenFactory: ()=>user.token
      })
      .withAutomaticReconnect()
      .build();
      this.hubConnection.start().catch(error=>console.log(error));

      this.hubConnection.on('UserIsOnline', username =>{
        //this.toastr.info(username + ' has connected');
        this.onlineUsers$.pipe(take(1)).subscribe({
          next: usernames=> this.onlineUsersSource.next([...usernames, username])
        })
      })

      this.hubConnection.on('UserIsOffline', username=>{
        //this.toastr.warning(username + ' has disconnected');
        this.onlineUsers$.pipe(take(1)).subscribe({
          next: usernames=> this.onlineUsersSource.next(usernames.filter(x => x!==username ))//filter method creates a new array
        })
      })

      this.hubConnection.on('GetOnlineUsers',usernames =>{
        this.onlineUsersSource.next(usernames);
      })
      this.hubConnection.on('NewMessageReceived',({username, knownAs}) =>{
        this.toastr.info(knownAs + ' has sent you a new message! Click me to see it!')
        .onTap.pipe(take(1))
        .subscribe({
          next:()=> 
          this.router.navigateByUrl('/RefreshComponent', { skipLocationChange: true }) // a trick to reload member detail component
          //when at member-detail, (ex at the info page), we cannot go to messages tab
            .then(() => {this.router.navigateByUrl('/members/' + username + '?tab=Messages')})
        })

        this.unreadMessagesNum$.pipe(take(1))
        .subscribe({
          next:unreadNum=> this.unreadMessagesNumSource.next(unreadNum+1)
        })

      })


      this.hubConnection.on('GetUnreadMessagesNumber',unreadNum =>{
        this.unreadMessagesNumSource.next(unreadNum);
      })

      this.hubConnection.on('ReadMessages', unreadNum =>{

        this.unreadMessagesNum$.pipe(take(1)).subscribe({
          next: LastunreadNum=>{
            this.unreadMessagesNumSource.next(LastunreadNum-unreadNum)
          }
        })
      })
      
  }

  stopHubConnection(){
    this.hubConnection?.stop().catch(error=>console.log(error));
  }
  

}
