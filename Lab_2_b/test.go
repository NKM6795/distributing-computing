 package main

import (
  "fmt"
)

var task_list = []string{ "task_0", "task_1", "task_2", "task_3", "task_4", "task_5", "task_6", "task_7", "task_8", "task_9" }
var size = len(task_list)

var take_away = make(chan string)
var load = make(chan string)

var done = make(chan bool)

func first_worker() {
    for i := 0; i < size; i++ {
        take_away <- task_list[i]
    }
}

func second_worker() {
    for i := 0; i < size; i++ {
        task := <-take_away
        load <- task
    }
}

func third_worker() {
    for i := 0; i < size; i++ {
        task := <-load
        // calculating
        fmt.Println(task)
    }
    done <- true
}

func main () {
    go first_worker()
    go second_worker()
    go third_worker()
    <-done
}
