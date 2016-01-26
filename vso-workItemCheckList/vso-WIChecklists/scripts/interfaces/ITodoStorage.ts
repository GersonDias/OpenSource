/// <reference path='../_all.ts' />

module todos {
    export interface ITodoStorage {
        get(workItemId: number): todoItem[];
        put(todos: todoItem[], workItemId: number);
    }
}