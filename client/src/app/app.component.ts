import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';
import { PresenceService } from './_services/presence.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title= 'Dating App';
  users: any;
  unread: number|undefined;
  
  constructor(private accountService: AccountService) {}
  ngOnInit(): void {
    //this.getUsers();
    this.setCurrentUser();
    
  }

  setCurrentUser(){
    const userString = localStorage.getItem('user');
    if(!userString) return;
    const user:User = JSON.parse(userString);
    this.accountService.setCurrentUser(user);
  }
  
}
