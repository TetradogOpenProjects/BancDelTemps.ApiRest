<?php

namespace Database\Factories;

use App\Models\UserEvent;
use App\Models\User;
use App\Models\Event;
use Illuminate\Database\Eloquent\Factories\Factory;

class UserEventFactory extends Factory
{
    /**
     * The name of the factory's corresponding model.
     *
     * @var string
     */
    protected $model = UserEvent::class;

    /**
     * Define the model's default state.
     *
     * @return array
     */
    public function definition()
    {
        return [
            'user_id'=>User::factory(),
            'event_id'=>Event::factory(),
            'approvedBy_id'=>User::factory(),
            'assisted'=>$this->faker->boolean()
        ];
    }
}
