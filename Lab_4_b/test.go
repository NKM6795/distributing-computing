
package main

import (
	"fmt"
	"math/rand"
	"time"
	"sync"
    "os"

)

var rm_lock sync.RWMutex

var garden = [][]string{ 
	{ "good", "good", "good" },
	{ "good", "bad", "good" },
	{ "good", "good", "good" }}

var iteration_count = 10

var done = make(chan bool, 2)

func gardener() {
	for {
		rm_lock.Lock()

		for i := 0; i < len(garden); i++ {
			for j := 0; j < len(garden[i]); j++ {
				if garden[i][j] == "bad" {
					garden[i][j] = "good"
				}
			}
		}

		rm_lock.Unlock()

		time.Sleep(10 * time.Millisecond)
	}
}

func nature() {
	for {
		rm_lock.Lock()

		for i := 0; i < len(garden); i++ {
			for j := 0; j < len(garden[i]); j++ {
				// 0 - no problem
				// 1 - tree => bad
				// 2 - tree => dead
				problem := rand.Intn(3)
				if problem == 1 {
					garden[i][j] = "bad"
				}
				if problem == 2 {
					garden[i][j] = "dead"
 				}
			}
		}

		rm_lock.Unlock()

		time.Sleep(10 * time.Millisecond)
	}
}
	
func monitor_1() {
	file_output, err := os.Create("data.dat")
	if err != nil {
		panic(err)
	}

	for i := 0; i < iteration_count; i++ {
		
		rm_lock.RLock()

		for _, row := range garden {
			for _, cell := range row {
				file_output.WriteString(cell + " ")
			}
			file_output.WriteString("\n")
		}
		file_output.WriteString("\n")

		rm_lock.RUnlock()

		time.Sleep(10 * time.Millisecond)
	}

	file_output.Close()

	done <- true
}

func monitor_2() {
	for i := 0; i < iteration_count; i++ {
		rm_lock.RLock()

		for _, row := range garden {
			for _, cell := range row {
				fmt.Print(cell, " ")
			}
			fmt.Println()
		}
		fmt.Println()

		rm_lock.RUnlock()

		time.Sleep(10 * time.Millisecond)
	}

	done <- true
}

func main() {
	go gardener()
	go nature()
	go monitor_1()
	go monitor_2()

	<-done
	<-done
}
