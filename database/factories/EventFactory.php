<?php

namespace Database\Factories;

use App\Models\Event;
use App\Models\User;
use App\Models\Location;
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
            
           'user_id'        =>User::factory(),
           'location_id'    =>Location::factory(),
           'approvedBy_id'  =>User::factory(),

           'title'          =>$this->faker->sentence(),
           'description'    =>$this->faker->paragraph(),
           'numMinUsers'    =>$this->faker->randomDigit(),
           'numMaxUsers'    =>$this->faker->randomDigit(),
           'minPrice'       =>$this->faker->randomDigit(),
           'initDate'       =>$this->faker->date(),
           'time'           =>$this->faker->randomDigit()
        ];
    }
}
