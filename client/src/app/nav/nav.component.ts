import { Component, Input, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { PresenceService } from '../_services/presence.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  model:any={};
 // currentUser$: Observable<User|null> =of(null); // to init a observable
  constructor(public accountService: AccountService, private router: Router,private toastr: ToastrService, public presenceService: PresenceService) { }

  ngOnInit(): void {

  }

  login(){

    if (!this.model.username) {
      this.toastr.error('Please fill in the username');
       return;
    }

    this.accountService.login(this.model).subscribe({
      next: _ => {
        this.router.navigateByUrl('/members') //no argument
        this.model ={};
      }
      
    })
    //http request after completed automatically unsubscribe
  }

  logout(){
    this.accountService.logout();
    this.router.navigateByUrl('/')
  }
}
