import { HttpClient, HttpParams } from "@angular/common/http";
import { map } from "rxjs";
import { PaginatedResult } from "../_models/pagination";


export function getPaginationHeaders(pageNumber: number, pageSize: number) {
    let params = new HttpParams(); //set query string along with our http request
    params = params.append('pageNumber', pageNumber);
    params = params.append('pageSize', pageSize);
    return params;
  }

export function getPaginatedResult<T>(url: string, params: HttpParams, http:HttpClient) {
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>;
    return http.get<T>(url, { observe: 'response', params }).pipe(
      //we want to see pagination header and want to pass in the params, not only the body
      map(response => {
        if (response.body) {
          paginatedResult.result = response.body;
        }
        const pagination = response.headers.get('Pagination');
        if (pagination) {
          paginatedResult.pagination = JSON.parse(pagination);
        }
        return paginatedResult;
      })
    );
}