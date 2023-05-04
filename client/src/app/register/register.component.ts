import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import {interval, timer, Observable, Subject, of, from, BehaviorSubject, ReplaySubject} from 'rxjs';
import { map, tap, takeUntil} from 'rxjs/operators';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

  @Output() cancelRegister = new EventEmitter();

  registerForm: FormGroup = new FormGroup({})
  private destroy$ = new ReplaySubject<boolean>(1);
  // The replaySubject will help to keep the component in an destroyed state even if you try to use any of the observable after ngOnDestroy has already been called. 
  maxDate: Date = new Date();
  validationErrors: string[] | undefined
  constructor(private accountService:AccountService,private toastr: ToastrService, 
    private fb: FormBuilder, private router:Router) { }

  ngOnInit(): void {
    console.log("new Date()",new Date())
    this.initializeForm();
    this.maxDate.setFullYear(this.maxDate.getFullYear()-18);
  }

  ngOnDestroy(){
    this.destroy$.next(true);
    this.destroy$.complete(); 
  }
  initializeForm(){
    
    // this.registerForm = new FormGroup({
    //   username: new FormControl('',Validators.required),//1. initial value 2.add validators
    //   password: new FormControl('',[Validators.required,Validators.minLength(4),Validators.maxLength(8)]),
    //   confirmPassword: new FormControl('',[Validators.required, this.matchValues('password')])
    // })
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['',Validators.required],
      knownAs: ['',Validators.required],
      dateOfBirth: ['',Validators.required],
      city: ['',Validators.required],
      country: ['',Validators.required],//1. initial value 2.add validators
      password: ['',[Validators.required,Validators.minLength(4),Validators.maxLength(8)]],
      confirmPassword: ['',[Validators.required, this.matchValues('password')]]
    })
    //FormControl are input
    //https://rxjs.dev/api/operators/takeUntil
    this.registerForm.controls['password'].valueChanges.pipe(takeUntil(this.destroy$))
    .subscribe({
      next: () => this.registerForm.controls['confirmPassword'].updateValueAndValidity()
    })

  }

  matchValues(matchTo: string): ValidatorFn{
    return (control: AbstractControl)=>{
      return control.value ===control.parent?.get(matchTo)?.value? null : {notMatching:true}
    }
  }
  register(){
    //console.log(this.registerForm?.value);
    const dob = this.getDateOnly(this.registerForm.controls['dateOfBirth'].value)
    const values = {...this.registerForm.value, dateOfBirth:dob};
    this.accountService.register(values).subscribe({
      next:()=>{
        this.router.navigateByUrl('/members')
      },
      error: error=>{
        this.validationErrors = error //server side error
        //console.log(error); //error handling
      }
    })
  }
  cancel(){
    console.log('cancelled');
    this.cancelRegister.emit(false);
  }

  private getDateOnly(dob: string|undefined)//from the form =>string
  {
    if(!dob) return
    let theDob = new Date(dob);
    return new Date(theDob.setMinutes(theDob.getMinutes()-theDob.getTimezoneOffset())).toISOString().slice(0,10);
//toISOString use GMT time. now I am 4-14, in UK it is 4-15, then we have a day's gap.
//1. make the GMT time the same as local time.
  }
}
