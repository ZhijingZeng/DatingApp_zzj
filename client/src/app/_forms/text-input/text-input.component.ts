import { Component, Input, OnInit, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl } from '@angular/forms';

@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  styleUrls: ['./text-input.component.css']
})
export class TextInputComponent implements ControlValueAccessor {
  @Input() label ='';
  @Input() type = 'text';

  constructor(@Self() public ngControl:NgControl) {
    this.ngControl.valueAccessor =this; //'this' means the class TextInputComponent
   }
  //  Angular will attempt to re-use services from anywhere in the component tree.   
  //  When we use @self we tell Angular that this is a service that we do not want to re-use 
  //  and to create an instance specifically for this component.   
  //  We do not want to re-use the ngControl here and needs to be unique for each input.

  writeValue(obj: any): void {


  }


  registerOnChange(fn: any): void {

  }


  registerOnTouched(fn: any): void {

  }
  get control(): FormControl{
    return this.ngControl.control as FormControl
  }

}
