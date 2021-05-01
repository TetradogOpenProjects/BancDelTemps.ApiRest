<?php

namespace Database\Factories;

use App\Models\Event;
use Illuminate\Database\Eloquent\Factories\Factory;

class EventFactory extends Factory
{
    /**
     * The name of the factory's corresponding model.
     *
     * @var string
     */
    protected $model = Event::class;

    /**
     * Define the model's default state.
     *
     * @return array
     */
    public function definition()
    {
        return [
            
           'user_id'        =>User::Factory(),
           'location_id'    =>Location::Factory(),
           'approvedBy_id'  =>User::Factory(),

           'title'          =>$this->faker->paragraph,
           'description'    =>$this->faker->paragraph,
           'numMinUsers'    =>$this->faker->random_int(1,15),
           'numMaxUsers'    =>$this->faker->random_int(15,25),
           'minPrice'       =>$this->faker->random_int(1,9),
           'initDate'       =>$this->faker->date(),
           'time'           =>$this->faker->random_int(1,15)
        ];
    }
}
